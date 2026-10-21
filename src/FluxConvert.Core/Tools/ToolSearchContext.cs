using FluxConvert.Core.Settings;

namespace FluxConvert.Core.Tools;

/// <summary>
/// Describes where the external programs may be found.
/// </summary>
public sealed class ToolSearchContext
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ToolSearchContext"/> class.
    /// </summary>
    public ToolSearchContext()
    {
    }

    /// <summary>Explicit path to <c>ffmpeg.exe</c> configured by the user.</summary>
    public string? ExplicitFfmpegPath { get; init; }

    /// <summary>Explicit path to <c>ffprobe.exe</c> configured by the user.</summary>
    public string? ExplicitFfprobePath { get; init; }

    /// <summary>Explicit path to <c>magick.exe</c> configured by the user.</summary>
    public string? ExplicitImageMagickPath { get; init; }

    /// <summary>
    /// Directories that are searched for a bundled copy, for example the application folder and
    /// the repository's <c>tools</c> folder during development.
    /// </summary>
    public IReadOnlyList<string> SearchRoots { get; init; } = Array.Empty<string>();

    /// <summary>Directories taken from the system <c>PATH</c> variable.</summary>
    public IReadOnlyList<string> PathDirectories { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Builds the default search context for the current machine.
    /// </summary>
    /// <param name="settings">The user settings, which may point at a specific executable.</param>
    /// <param name="baseDirectory">The folder the application was started from.</param>
    /// <returns>The search context.</returns>
    public static ToolSearchContext CreateDefault(AppSettings settings, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var roots = new List<string>();

        if (!string.IsNullOrWhiteSpace(baseDirectory))
        {
            roots.Add(baseDirectory);
            roots.Add(Path.Combine(baseDirectory, "tools"));
        }

        var repositoryRoot = FindRepositoryRoot(baseDirectory);
        if (repositoryRoot is not null)
        {
            roots.Add(repositoryRoot);
            roots.Add(Path.Combine(repositoryRoot, "tools"));
        }

        return new ToolSearchContext
        {
            ExplicitFfmpegPath = NullIfBlank(settings.FfmpegPath),
            ExplicitFfprobePath = DeriveSiblingExecutable(settings.FfmpegPath, "ffprobe.exe"),
            ExplicitImageMagickPath = NullIfBlank(settings.ImageMagickPath),
            SearchRoots = roots,
            PathDirectories = ReadPathDirectories(),
        };
    }

    /// <summary>
    /// Works out where a companion executable lives next to a user-configured one. FFmpeg and
    /// FFprobe are always shipped together, so pointing at <c>ffmpeg.exe</c> is enough to find
    /// <c>ffprobe.exe</c> as well.
    /// </summary>
    private static string? DeriveSiblingExecutable(string? configuredPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        var trimmed = configuredPath.Trim();
        var directory = trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? Path.GetDirectoryName(trimmed)
            : trimmed;

        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        return Path.Combine(directory, fileName);
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] ReadPathDirectories()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return Array.Empty<string>();
        }

        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(directory => !string.IsNullOrWhiteSpace(directory))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? FindRepositoryRoot(string startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
        {
            return null;
        }

        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (directory.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly).Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
