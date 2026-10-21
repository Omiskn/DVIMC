using System.ComponentModel;
using System.Windows;
using FluxConvert.App.Composition;
using FluxConvert.App.Shell;
using FluxConvert.Core.Settings;
using FluxConvert.Core.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace FluxConvert.App;

/// <summary>
/// Application entry point and owner of the application shell.
/// </summary>
public partial class App : Application, IDisposable
{
    private ServiceProvider? _services;
    private SingleInstanceCoordinator? _singleInstance;
    private TrayIconController? _tray;
    private IDisposable? _activationListener;
    private ShellWindow? _shell;
    private bool _disposed;

    /// <summary>
    /// Whether the application is shutting down for good. The shell uses this to decide whether
    /// closing the window should hide it or end the process.
    /// </summary>
    public bool IsExiting { get; private set; }

    /// <summary>Initialises the application.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The application lives in the notification area, so closing the visible window must not
        // end the process.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _singleInstance = new SingleInstanceCoordinator(SingleInstanceCoordinator.DefaultApplicationName);
        if (!_singleInstance.IsPrimary)
        {
            // Another copy is already running. Ask it to show itself and step aside.
            SingleInstanceCoordinator.SignalPrimary(SingleInstanceCoordinator.DefaultApplicationName);
            Shutdown();
            return;
        }

        _services = AppServices.Build();

        var settings = _services.GetRequiredService<ISettingsService>();
        settings.Load();

        _tray = new TrayIconController(ShowShell, RequestExit);
        _tray.Show();

        _activationListener = _singleInstance.StartActivationListener(
            () => Dispatcher.Invoke(ShowShell));

        _shell = new ShellWindow();
        MainWindow = _shell;

        if (!settings.Current.StartMinimizedToTray)
        {
            _shell.Show();
        }
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _activationListener?.Dispose();
        _activationListener = null;

        _tray?.Dispose();
        _tray = null;

        _singleInstance?.Dispose();
        _singleInstance = null;

        _services?.Dispose();
        _services = null;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Brings the interface to the front, creating it if necessary.
    /// </summary>
    public void ShowShell()
    {
        if (_shell is null)
        {
            return;
        }

        if (!_shell.IsVisible)
        {
            _shell.Show();
        }

        if (_shell.WindowState == WindowState.Minimized)
        {
            _shell.WindowState = WindowState.Normal;
        }

        _shell.Activate();
        _shell.Topmost = true;
        _shell.Topmost = false;
        _shell.Focus();
    }

    /// <summary>
    /// Quits the application, asking for confirmation when a conversion is still running.
    /// </summary>
    public void RequestExit()
    {
        var activity = _services?.GetService<IConversionActivity>();
        if (activity is { IsBusy: true })
        {
            var answer = MessageBox.Show(
                "A conversion is still running. Quitting now will cancel it." + Environment.NewLine + Environment.NewLine +
                "Quit anyway?",
                "FluxConvert",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        IsExiting = true;
        Shutdown();
    }
}
