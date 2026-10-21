using System.Drawing;
using System.Windows.Forms;

namespace FluxConvert.App.Shell;

/// <summary>
/// The notification-area icon and its context menu.
/// </summary>
/// <remarks>
/// <para>
/// The icon is a <see cref="System.Windows.Forms.NotifyIcon"/>. Windows Forms is used for this
/// one control only — the rest of the interface stays in WPF. It is the supported way to place a
/// notification-area icon in a WPF application without re-implementing <c>Shell_NotifyIcon</c>
/// and its version negotiation by hand.
/// </para>
/// <para>
/// The icon is never shown as a taskbar button: the application lives in the notification area
/// and the floating interface is brought up on demand.
/// </para>
/// </remarks>
public sealed class TrayIconController : IDisposable
{
    private const int MaxToolTipLength = 63;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly IconLease _icon;
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="TrayIconController"/> class.
    /// </summary>
    /// <param name="onShow">Invoked when the user asks for the interface to be shown.</param>
    /// <param name="onExit">Invoked when the user asks the application to quit.</param>
    public TrayIconController(Action onShow, Action onExit)
    {
        ArgumentNullException.ThrowIfNull(onShow);
        ArgumentNullException.ThrowIfNull(onExit);

        _icon = AppIconFactory.CreateIcon(32);

        _menu = new ContextMenuStrip { ShowImageMargin = false };
        _menu.Items.Add("Show FluxConvert", null, (_, _) => onShow());
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Exit", null, (_, _) => onExit());

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon.Icon,
            Text = "FluxConvert",
            Visible = false,
            ContextMenuStrip = _menu,
        };

        _notifyIcon.DoubleClick += (_, _) => onShow();
    }

    /// <summary>The text shown when the pointer rests on the icon.</summary>
    public string ToolTip
    {
        get => _notifyIcon.Text;
        set => _notifyIcon.Text = Truncate(value);
    }

    /// <summary>Shows the icon in the notification area.</summary>
    public void Show() => _notifyIcon.Visible = true;

    /// <summary>Removes the icon from the notification area.</summary>
    public void Hide() => _notifyIcon.Visible = false;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _icon.Dispose();
    }

    private static string Truncate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= MaxToolTipLength ? value : value[..MaxToolTipLength];
    }
}
