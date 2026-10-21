using System.Runtime.InteropServices;
using FluxConvert.Core.Infrastructure;
using FluxConvert.Core.Settings;
using FluxConvert.Core.Tools;

namespace FluxConvert.Core.Diagnostics;

/// <summary>
/// Builds the diagnostics report from the application's own state.
/// </summary>
public static class DiagnosticsReportBuilder
{
    /// <summary>
    /// Builds the report.
    /// </summary>
    /// <param name="paths">The application data paths.</param>
    /// <param name="settings">The effective settings.</param>
    /// <param name="tools">The media tools that were located.</param>
    /// <returns>The report.</returns>
    public static DiagnosticsReport Build(
        AppPaths paths,
        AppSettings settings,
        IEnumerable<ExternalTool> tools)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(tools);

        var report = new DiagnosticsReport { Title = $"{ProductInfo.ProductName} {ProductInfo.Version}" };

        report.AddSection(BuildSystemSection());
        report.AddSection(BuildPathsSection(paths));
        report.AddSection(BuildToolsSection(tools));
        report.AddSection(BuildSettingsSection(settings));

        return report;
    }

    private static DiagnosticsSection BuildSystemSection()
    {
        var section = new DiagnosticsSection("System");
        section.Entries.Add(new DiagnosticsEntry(
            "Operating system",
            $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}",
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Architecture",
            RuntimeInformation.ProcessArchitecture.ToString(),
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            ".NET runtime",
            RuntimeInformation.FrameworkDescription,
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Application folder",
            AppContext.BaseDirectory,
            DiagnosticsStatus.Info));
        return section;
    }

    private static DiagnosticsSection BuildPathsSection(AppPaths paths)
    {
        var section = new DiagnosticsSection("Paths");
        section.Entries.Add(new DiagnosticsEntry("Data root", paths.Root, DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry("Settings file", paths.SettingsFile, DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry("Log directory", paths.LogDirectory, DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry("Temporary directory", paths.TempDirectory, DiagnosticsStatus.Info));
        return section;
    }

    private static DiagnosticsSection BuildToolsSection(IEnumerable<ExternalTool> tools)
    {
        var section = new DiagnosticsSection("Media tools");

        foreach (var tool in tools)
        {
            if (tool.IsAvailable)
            {
                section.Entries.Add(new DiagnosticsEntry(
                    tool.DisplayName,
                    tool.Version is null
                        ? $"found at {tool.ExecutablePath}"
                        : $"{tool.Version} ({tool.ExecutablePath})",
                    DiagnosticsStatus.Ok));
            }
            else
            {
                section.Entries.Add(new DiagnosticsEntry(
                    tool.DisplayName,
                    $"not available - {tool.UnavailableReason}",
                    DiagnosticsStatus.Warning));
            }
        }

        return section;
    }

    private static DiagnosticsSection BuildSettingsSection(AppSettings settings)
    {
        var section = new DiagnosticsSection("Settings");
        section.Entries.Add(new DiagnosticsEntry(
            "Default output folder",
            string.IsNullOrWhiteSpace(settings.DefaultOutputDirectory)
                ? "(next to the source file)"
                : settings.DefaultOutputDirectory,
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry("Quality preset", settings.QualityPreset, DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Max concurrent conversions",
            settings.MaxConcurrentConversions.ToString(),
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Strip metadata",
            settings.StripMetadata ? "yes" : "no",
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Reduced motion",
            settings.ReducedMotion ? "yes" : "no",
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry("Theme", settings.Theme, DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "Start hidden in tray",
            settings.StartMinimizedToTray ? "yes" : "no",
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "FFmpeg location",
            string.IsNullOrWhiteSpace(settings.FfmpegPath) ? "(discover automatically)" : settings.FfmpegPath,
            DiagnosticsStatus.Info));
        section.Entries.Add(new DiagnosticsEntry(
            "ImageMagick location",
            string.IsNullOrWhiteSpace(settings.ImageMagickPath) ? "(discover automatically)" : settings.ImageMagickPath,
            DiagnosticsStatus.Info));
        return section;
    }
}
