using FluxConvert.Core;
using System.Windows;

namespace FluxConvert.App;

/// <summary>
/// Application entry point.
/// </summary>
public partial class App : Application
{
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
        var shell = new ShellWindow();
        MainWindow = shell;
        shell.Show();
    }
}
