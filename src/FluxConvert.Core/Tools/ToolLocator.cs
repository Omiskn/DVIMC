using System.Diagnostics;
using FluxConvert.Core.Process;

namespace FluxConvert.Core.Tools;

/// <summary>
/// Finds <c>ffmpeg.exe</c>, <c>ffprobe.exe</c> and <c>magick.exe</c> on the machine.
/// </summary>
/// <remarks>
/// <para>The search order for every program is:</para>
/// <list type="number">
///   <item><description>the explicit path from the user settings;</description></item>
///   <item><description>a bundled copy in a known application folder;</description></item>
///   <item><description>the system <c>PATH</c>.</description></item>
/// </list>
/// <para>
/// A program that cannot be found is reported as unavailable with a reason. The locator never
/// throws, so a missing tool can never stop the application from starting.
/// </para>
/// <para>
/// Only <c>magick.exe</c> is looked for for ImageMagick. The legacy <c>convert.exe</c> name is
/// deliberately ignored: on Windows it refers to the built-in file system conversion utility, not
/// to ImageMagick.
/// </para>
/// </remarks>
public sealed class ToolLocator : IToolLocator
{
    private static readonly string[] FfmpegRelativeDirectories = { Path.Combine("ffmpeg", "bin"), "ffmpeg" };
    private static readonly string[] ImageMagickRelativeDirectories = { "imagemagick", "ImageMagick" };

    private static readonly TimeSpan VersionProbeTimeout = TimeSpan.FromSeconds(10);

    private static readonly string[] VersionArguments = { "-version" };

    private readonly ToolSearchContext _context;
    private readonly IProcessRunner _processRunner;
    private readonly Dictionary<ExternalToolKind, ExternalTool> _cache = new();
    private readonly object _sync = new();

    /// <summary>
    /// Initialises a new instance of the <see cref="ToolLocator"/> class.
    /// </summary>
    /// <param name="context">Describes where the programs may be found.</param>
    /// <param name="processRunner">Used to read the version of a program that was found.</param>
    public ToolLocator(ToolSearchContext context, IProcessRunner processRunner)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    /// <inheritdoc />
    public async Task<ExternalTool> GetAsync(ExternalToolKind kind, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_cache.TryGetValue(kind, out var cached))
            {
                return cached;
            }
        }

        var located = await LocateAsync(kind, cancellationToken).ConfigureAwait(false);

        lock (_sync)
        {
            _cache[kind] = located;
        }

        return located;
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        lock (_sync)
        {
            _cache.Clear();
        }
    }

    private async Task<ExternalTool> LocateAsync(ExternalToolKind kind, CancellationToken cancellationToken)
    {
        var (displayName, fileName, explicitPath, relativeDirectories) = Describe(kind);

        var candidates = BuildCandidates(fileName, explicitPath, relativeDirectories);

        foreach (var candidate in candidates)
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            var version = await ProbeVersionAsync(candidate, cancellationToken).ConfigureAwait(false);
            return ExternalTool.Found(kind, displayName, candidate, version);
        }

        return ExternalTool.Missing(
            kind,
            displayName,
            $"No {fileName} was found. Set its location in Settings, add it to PATH, or place it in the application's tools folder.");
    }

    private (string DisplayName, string FileName, string? ExplicitPath, string[] RelativeDirectories) Describe(
        ExternalToolKind kind) => kind switch
        {
            ExternalToolKind.Ffmpeg => ("FFmpeg", "ffmpeg.exe", _context.ExplicitFfmpegPath, FfmpegRelativeDirectories),
            ExternalToolKind.Ffprobe => ("FFprobe", "ffprobe.exe", _context.ExplicitFfprobePath, FfmpegRelativeDirectories),
            ExternalToolKind.ImageMagick => ("ImageMagick", "magick.exe", _context.ExplicitImageMagickPath, ImageMagickRelativeDirectories),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown tool."),
        };

    private IEnumerable<string> BuildCandidates(string fileName, string? explicitPath, string[] relativeDirectories)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            yield return explicitPath;
        }

        foreach (var root in _context.SearchRoots)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            foreach (var relative in relativeDirectories)
            {
                yield return Path.Combine(root, relative, fileName);
            }
        }

        foreach (var directory in _context.PathDirectories)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            yield return Path.Combine(directory, fileName);
        }
    }

    private async Task<string?> ProbeVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _processRunner.RunAsync(
                new ProcessRequest(executablePath, VersionArguments, Timeout: VersionProbeTimeout),
                cancellationToken).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return null;
            }

            var line = FirstNonEmptyLine(result.StandardOutput) ?? FirstNonEmptyLine(result.StandardError);
            return line?.Trim();
        }
        catch (ExternalToolException)
        {
            return null;
        }
        catch (TimeoutException)
        {
            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    private static string? FirstNonEmptyLine(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }

        return null;
    }
}
