using FluxConvert.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FluxConvert.Core.Tests;

public class FileLoggerProviderTests : IDisposable
{
    private readonly string _directory;

    public FileLoggerProviderTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "fluxconvert-log-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void BuildFileName_UsesTheDateOnly()
    {
        var name = FileLoggerProvider.BuildFileName(new DateTimeOffset(2026, 3, 7, 21, 45, 12, TimeSpan.Zero));

        Assert.Equal("fluxconvert-20260307.log", name);
    }

    [Fact]
    public void Writes_IntoADailyFile()
    {
        var timestamp = new DateTimeOffset(2026, 3, 7, 21, 45, 12, TimeSpan.Zero);
        using var provider = new FileLoggerProvider(_directory, LogLevel.Information, () => timestamp);
        var logger = provider.CreateLogger("FluxConvert.Test.Category");

        logger.LogInformation("Conversion finished: {File}", "sample.png");

        var expected = Path.Combine(_directory, "fluxconvert-20260307.log");
        Assert.True(File.Exists(expected));

        var content = File.ReadAllText(expected);
        Assert.Contains("Conversion finished: sample.png", content, StringComparison.Ordinal);
        Assert.Contains("FluxConvert.Test.Category", content, StringComparison.Ordinal);
        Assert.Contains("[INF]", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Respects_TheMinimumLevel()
    {
        using var provider = new FileLoggerProvider(_directory, LogLevel.Warning);
        var logger = provider.CreateLogger("FluxConvert.Test.Category");

        logger.LogDebug("debug message");
        logger.LogInformation("information message");
        logger.LogWarning("warning message");

        var files = Directory.GetFiles(_directory, "*.log");
        Assert.Single(files);

        var content = File.ReadAllText(files[0]);
        Assert.DoesNotContain("debug message", content, StringComparison.Ordinal);
        Assert.DoesNotContain("information message", content, StringComparison.Ordinal);
        Assert.Contains("warning message", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Includes_TheExceptionDetails()
    {
        using var provider = new FileLoggerProvider(_directory, LogLevel.Information);
        var logger = provider.CreateLogger("FluxConvert.Test.Category");

        logger.LogError(new InvalidOperationException("boom"), "something failed");

        var files = Directory.GetFiles(_directory, "*.log");
        Assert.Single(files);

        var content = File.ReadAllText(files[0]);
        Assert.Contains("something failed", content, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", content, StringComparison.Ordinal);
    }

    [Fact]
    public void IsEnabled_ReflectsTheMinimumLevel()
    {
        using var provider = new FileLoggerProvider(_directory, LogLevel.Warning);

        Assert.False(provider.IsEnabled(LogLevel.Information));
        Assert.True(provider.IsEnabled(LogLevel.Warning));
        Assert.False(provider.IsEnabled(LogLevel.None));
    }
}
