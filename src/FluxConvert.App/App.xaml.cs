using System.Windows;
using FluxConvert.App.Composition;
using FluxConvert.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace FluxConvert.App;

/// <summary>
/// Application entry point.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    /// <summary>Initialises the application.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The window is created explicitly rather than through `StartupUri` so that startup
        // behaviour (tray-only start, diagnostics mode, single-instance handling) can be
        // introduced without restructuring the entry point.
        _services = AppServices.Build();

        var settings = _services.GetRequiredService<ISettingsService>();
        settings.Load();

        var shell = new ShellWindow();
        MainWindow = shell;
        shell.Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        // Disposing the provider flushes and closes the file logger.
        _services?.Dispose();
        _services = null;
        base.OnExit(e);
    }
}
