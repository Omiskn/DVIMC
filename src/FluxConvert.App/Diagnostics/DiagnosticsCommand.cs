using FluxConvert.Core;
using FluxConvert.Core.Diagnostics;
using FluxConvert.Core.Infrastructure;
using FluxConvert.Core.Settings;
using FluxConvert.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace FluxConvert.App.Diagnostics;

/// <summary>
/// Handles the command-line switches the application understands.
/// </summary>
public static class DiagnosticsCommand
{
    /// <summary>The switch that prints the diagnostics report.</summary>
    public const string DiagnosticsSwitch = "--diagnostics";

    /// <summary>The switch that prints the usage text.</summary>
    public const string HelpSwitch = "--help";

    /// <summary>
    /// Whether the given arguments ask for something other than the normal interface.
    /// </summary>
    /// <param name="arguments">The command-line arguments.</param>
    /// <returns><see langword="true"/> when a command-line mode was requested.</returns>
    public static bool IsCommandLineMode(IReadOnlyList<string> arguments) =>
        arguments.Any(argument =>
            string.Equals(argument, DiagnosticsSwitch, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(argument, HelpSwitch, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Runs the requested command-line mode.
    /// </summary>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="services">The application's service provider.</param>
    /// <returns>The exit code the process should use.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> arguments, ServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(services);

        ConsoleOutput.Ensure();

        if (arguments.Any(argument => string.Equals(argument, HelpSwitch, StringComparison.OrdinalIgnoreCase)))
        {
            WriteUsage();
            ConsoleOutput.WaitForAcknowledgement();
            return 0;
        }

        var report = await BuildReportAsync(services).ConfigureAwait(false);
        Console.Write(report.Format());
        ConsoleOutput.WaitForAcknowledgement();
        return 0;
    }

    private static async Task<DiagnosticsReport> BuildReportAsync(ServiceProvider services)
    {
        var paths = services.GetRequiredService<AppPaths>();
        var settings = services.GetRequiredService<ISettingsService>();
        var locator = services.GetRequiredService<IToolLocator>();

        settings.Load();

        var tools = new List<ExternalTool>
        {
            await locator.GetAsync(ExternalToolKind.Ffmpeg).ConfigureAwait(false),
            await locator.GetAsync(ExternalToolKind.Ffprobe).ConfigureAwait(false),
            await locator.GetAsync(ExternalToolKind.ImageMagick).ConfigureAwait(false),
        };

        return DiagnosticsReportBuilder.Build(paths, settings.Current, tools);
    }

    private static void WriteUsage()
    {
        Console.WriteLine($"{ProductInfo.ProductName} {ProductInfo.Version}");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine($"  {ProductInfo.ProductName}.exe {DiagnosticsSwitch}   Print a diagnostics report.");
        Console.WriteLine($"  {ProductInfo.ProductName}.exe {HelpSwitch}        Show this text.");
        Console.WriteLine();
        Console.WriteLine("With no switch the floating interface is shown.");
    }
}
