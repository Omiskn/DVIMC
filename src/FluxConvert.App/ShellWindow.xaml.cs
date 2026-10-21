using System.ComponentModel;
using System.Windows;

namespace FluxConvert.App;

/// <summary>
/// Temporary application shell window. Replaced by the floating radial widget in Phase 2.
/// </summary>
public partial class ShellWindow : Window
{
    /// <summary>Initialises the window.</summary>
    public ShellWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs e)
    {
        if (Application.Current is App { IsExiting: false })
        {
            // Closing the interface hides it; the application keeps running in the notification
            // area and can be brought back from the tray icon.
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
