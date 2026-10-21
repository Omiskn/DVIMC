using FluxConvert.Core;
using FluxConvert.Core.Diagnostics;
using FluxConvert.Core.Infrastructure;
using FluxConvert.Core.Settings;
using FluxConvert.Core.Tools;
using Xunit;

namespace FluxConvert.Core.Tests;

public class DiagnosticsReportTests
{
    private static DiagnosticsReport BuildReport(AppSettings? settings = null, params ExternalTool[] tools)
    {
        var root = Path.Combine(Path.GetTempPath(), "fluxconvert-diag-" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(root);
        return DiagnosticsReportBuilder.Build(paths, settings ?? new AppSettings(), tools);
    }

    [Fact]
    public void Report_ContainsTheDataRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "fluxconvert-diag-" + Guid.NewGuid().ToString("N"));

        var report = DiagnosticsReportBuilder.Build(new AppPaths(root), new AppSettings(), Array.Empty<ExternalTool>());

        Assert.Contains(root, report.Format(), StringComparison.Ordinal);
    }

    [Fact]
    public void Report_ListsEveryMediaTool()
    {
        var report = BuildReport(
            tools: new[]
            {
                ExternalTool.Missing(ExternalToolKind.Ffmpeg, "FFmpeg", "not installed"),
                ExternalTool.Missing(ExternalToolKind.Ffprobe, "FFprobe", "not installed"),
                ExternalTool.Missing(ExternalToolKind.ImageMagick, "ImageMagick", "not installed"),
            });

        var text = report.Format();

        Assert.Contains("FFmpeg", text, StringComparison.Ordinal);
        Assert.Contains("FFprobe", text, StringComparison.Ordinal);
        Assert.Contains("ImageMagick", text, StringComparison.Ordinal);
        Assert.Contains("not installed", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_ShowsTheVersionAndPathOfAnAvailableTool()
    {
        var report = BuildReport(
            tools: new[]
            {
                ExternalTool.Found(ExternalToolKind.Ffmpeg, "FFmpeg", @"C:\tools\ffmpeg.exe", "ffmpeg version 6.1.1"),
            });

        var text = report.Format();

        Assert.Contains("ffmpeg version 6.1.1", text, StringComparison.Ordinal);
        Assert.Contains(@"C:\tools\ffmpeg.exe", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_ShowsTheEffectiveSettings()
    {
        var settings = new AppSettings
        {
            QualityPreset = "HighQuality",
            MaxConcurrentConversions = 3,
            StripMetadata = false,
            ReducedMotion = true,
            DefaultOutputDirectory = @"D:\Out",
        };

        var text = BuildReport(settings).Format();

        Assert.Contains("HighQuality", text, StringComparison.Ordinal);
        Assert.Contains("3", text, StringComparison.Ordinal);
        Assert.Contains(@"D:\Out", text, StringComparison.Ordinal);
        Assert.Contains("Reduced motion", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_SaysWhereOutputGoesWhenNoFolderIsConfigured()
    {
        var text = BuildReport().Format();

        Assert.Contains("next to the source file", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_HasTheExpectedSections()
    {
        var report = BuildReport();

        var headings = report.Sections.Select(section => section.Heading).ToArray();

        Assert.Equal(new[] { "System", "Paths", "Media tools", "Settings" }, headings);
    }

    [Fact]
    public void Format_ProducesATitleAndASectionPerGroup()
    {
        var text = BuildReport().Format();

        Assert.StartsWith(ProductInfo.ProductName, text, StringComparison.Ordinal);
        Assert.Contains("System", text, StringComparison.Ordinal);
        Assert.Contains("Media tools", text, StringComparison.Ordinal);
        Assert.Contains("Settings", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_CopesWithASectionThatHasNoEntries()
    {
        var report = new DiagnosticsReport("Empty report", new[] { new DiagnosticsSection("Nothing") });

        var text = report.Format();

        Assert.Contains("nothing to report", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductVersion_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(ProductInfo.Version));
    }
}
