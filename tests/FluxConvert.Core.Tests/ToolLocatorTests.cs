using FluxConvert.Core.Process;
using FluxConvert.Core.Tools;
using Xunit;

namespace FluxConvert.Core.Tests;

public class ToolLocatorTests : IDisposable
{
    private readonly string _root;

    public ToolLocatorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "fluxconvert-tools-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private string CreateFile(string relativePath)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "not a real executable");
        return path;
    }

    private static FakeProcessRunner CreateRunner() => new();

    [Fact]
    public async Task ExplicitPathWinsOverEverythingElse()
    {
        var explicitPath = CreateFile(Path.Combine("explicit", "ffmpeg.exe"));
        CreateFile(Path.Combine("bundled", "ffmpeg", "bin", "ffmpeg.exe"));
        CreateFile(Path.Combine("onpath", "ffmpeg.exe"));

        var context = new ToolSearchContext
        {
            ExplicitFfmpegPath = explicitPath,
            SearchRoots = new[] { Path.Combine(_root, "bundled") },
            PathDirectories = new[] { Path.Combine(_root, "onpath") },
        };
        var locator = new ToolLocator(context, CreateRunner());

        var tool = await locator.GetAsync(ExternalToolKind.Ffmpeg);

        Assert.True(tool.IsAvailable);
        Assert.Equal(explicitPath, tool.ExecutablePath);
    }

    [Fact]
    public async Task BundledCopyWinsOverPath()
    {
        var bundled = CreateFile(Path.Combine("bundled", "ffmpeg", "bin", "ffmpeg.exe"));
        CreateFile(Path.Combine("onpath", "ffmpeg.exe"));

        var context = new ToolSearchContext
        {
            SearchRoots = new[] { Path.Combine(_root, "bundled") },
            PathDirectories = new[] { Path.Combine(_root, "onpath") },
        };
        var locator = new ToolLocator(context, CreateRunner());

        var tool = await locator.GetAsync(ExternalToolKind.Ffmpeg);

        Assert.True(tool.IsAvailable);
        Assert.Equal(bundled, tool.ExecutablePath);
    }

    [Fact]
    public async Task PathIsUsedAsTheLastResort()
    {
        var onPath = CreateFile(Path.Combine("onpath", "ffmpeg.exe"));

        var context = new ToolSearchContext
        {
            SearchRoots = Array.Empty<string>(),
            PathDirectories = new[] { Path.Combine(_root, "onpath") },
        };
        var locator = new ToolLocator(context, CreateRunner());

        var tool = await locator.GetAsync(ExternalToolKind.Ffmpeg);

        Assert.True(tool.IsAvailable);
        Assert.Equal(onPath, tool.ExecutablePath);
    }

    [Fact]
    public async Task AMissingToolIsReportedAsUnavailableInsteadOfThrowing()
    {
        var context = new ToolSearchContext
        {
            SearchRoots = Array.Empty<string>(),
            PathDirectories = Array.Empty<string>(),
        };
        var locator = new ToolLocator(context, CreateRunner());

        var tool = await locator.GetAsync(ExternalToolKind.ImageMagick);

        Assert.False(tool.IsAvailable);
        Assert.Null(tool.ExecutablePath);
        Assert.NotNull(tool.UnavailableReason);
        Assert.Contains("magick.exe", tool.UnavailableReason!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheVersionIsReadFromTheTool()
    {
        CreateFile(Path.Combine("bundled", "ffmpeg", "bin", "ffmpeg.exe"));
        var runner = CreateRunner();
        runner.VersionOutput = "ffmpeg version 6.1.1-1 Copyright (c) 2000-2023";

        var context = new ToolSearchContext
        {
            SearchRoots = new[] { Path.Combine(_root, "bundled") },
        };
        var locator = new ToolLocator(context, runner);

        var tool = await locator.GetAsync(ExternalToolKind.Ffmpeg);

        Assert.True(tool.IsAvailable);
        Assert.Equal("ffmpeg version 6.1.1-1 Copyright (c) 2000-2023", tool.Version);
        Assert.Contains("6.1.1", tool.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImageMagickIsNeverResolvedToTheWindowsConvertUtility()
    {
        // C:\Windows\System32\convert.exe is the file system conversion tool, not ImageMagick.
        CreateFile(Path.Combine("onpath", "convert.exe"));

        var context = new ToolSearchContext
        {
            PathDirectories = new[] { Path.Combine(_root, "onpath") },
        };
        var locator = new ToolLocator(context, CreateRunner());

        var tool = await locator.GetAsync(ExternalToolKind.ImageMagick);

        Assert.False(tool.IsAvailable);
    }

    [Fact]
    public async Task TheResultIsCachedUntilTheCacheIsInvalidated()
    {
        CreateFile(Path.Combine("bundled", "ffmpeg", "bin", "ffmpeg.exe"));
        var runner = CreateRunner();

        var context = new ToolSearchContext
        {
            SearchRoots = new[] { Path.Combine(_root, "bundled") },
        };
        var locator = new ToolLocator(context, runner);

        await locator.GetAsync(ExternalToolKind.Ffmpeg);
        await locator.GetAsync(ExternalToolKind.Ffmpeg);
        Assert.Single(runner.Requests);

        locator.Invalidate();
        await locator.GetAsync(ExternalToolKind.Ffmpeg);
        Assert.Equal(2, runner.Requests.Count);
    }

    [Fact]
    public async Task FfprobeIsLookedForSeparatelyFromFfmpeg()
    {
        CreateFile(Path.Combine("bundled", "ffmpeg", "bin", "ffmpeg.exe"));

        var context = new ToolSearchContext
        {
            SearchRoots = new[] { Path.Combine(_root, "bundled") },
        };
        var locator = new ToolLocator(context, CreateRunner());

        var ffmpeg = await locator.GetAsync(ExternalToolKind.Ffmpeg);
        var ffprobe = await locator.GetAsync(ExternalToolKind.Ffprobe);

        Assert.True(ffmpeg.IsAvailable);
        Assert.False(ffprobe.IsAvailable);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<ProcessRequest> Requests { get; } = new();

        public string VersionOutput { get; set; } = "tool version 1.0";

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(new ProcessResult(0, VersionOutput, string.Empty, TimeSpan.FromMilliseconds(1)));
        }
    }
}
