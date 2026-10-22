using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using FluxConvert.App.ViewModels;
using FluxConvert.Core.Layout;

namespace FluxConvert.App.Radial;

/// <summary>
/// The floating radial interface.
/// </summary>
/// <remarks>
/// <para>
/// The window is borderless and transparent, sits above every other window and never takes the
/// focus on its own. It is placed in the bottom right corner of the work area of the monitor
/// that holds the pointer, and is clamped so that it can never end up partly off-screen.
/// </para>
/// <para>
/// The keyboard model is spatial: an arrow key points at the item closest to that edge of the
/// ring. <c>Home</c> and <c>End</c> jump to the first and last available item, <c>Enter</c> and
/// <c>Space</c> choose the focused item, and <c>Escape</c> hides the widget and gives the focus
/// back to the window the user came from. All of the decisions are made by
/// <see cref="RingSelection"/>, which is unit tested; this class only moves real focus around.
/// </para>
/// </remarks>
public partial class RadialWindow : Window
{
    private const double WidgetMargin = 16.0;

    private IntPtr _previousForeground;

    /// <summary>Initialises a new instance of the <see cref="RadialWindow"/> class.</summary>
    public RadialWindow()
    {
        InitializeComponent();

        ViewModel = new RadialMenuViewModel();
        DataContext = ViewModel;

        BuildItems();

        Loaded += OnWindowLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
        GotKeyboardFocus += OnGotKeyboardFocus;
        LostKeyboardFocus += OnLostKeyboardFocus;
    }

    /// <summary>The state of the radial menu.</summary>
    public RadialMenuViewModel ViewModel { get; }

    /// <summary>
    /// Shows the widget and optionally gives it the focus.
    /// </summary>
    /// <param name="activate">
    /// <see langword="true"/> when the user explicitly summoned the widget, which is also when the
    /// window they came from is remembered so that <c>Escape</c> can return them to it.
    /// </param>
    public void ShowWidget(bool activate)
    {
        if (!IsVisible)
        {
            Show();
            PositionInWorkArea();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        if (!activate)
        {
            return;
        }

        if (_previousForeground == IntPtr.Zero)
        {
            _previousForeground = ForegroundWindowTracker.CapturePrevious();
        }

        Activate();

        // Windows only lets a process take the foreground when it received the last input. A
        // second launch or a signalled instance has not, so Activate() can silently do nothing.
        // The fallback attaches to the foreground thread and raises the window anyway.
        if (!IsActive)
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
            {
                _ = ForegroundWindowTracker.TryRestore(handle);
            }
        }

        Focus();
    }

    /// <summary>
    /// Places the widget in the corner of the work area of the monitor under the pointer.
    /// </summary>
    public void PositionInWorkArea()
    {
        var workArea = WorkAreaProvider.GetWorkAreaUnderCursor();
        var width = ActualWidth > 0 ? ActualWidth : Width;
        var height = ActualHeight > 0 ? ActualHeight : Height;

        if (double.IsNaN(width) || width <= 0)
        {
            width = 348;
        }

        if (double.IsNaN(height) || height <= 0)
        {
            height = 416;
        }

        var placed = RadialGeometry.PlaceInCorner(
            new RadialGeometry.Rect(0, 0, width, height),
            new RadialGeometry.Rect(workArea.X, workArea.Y, workArea.Width, workArea.Height),
            WidgetMargin,
            RadialGeometry.Corner.BottomRight);

        Left = placed.X;
        Top = placed.Y;
    }

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        // Alt+F4 must not destroy the widget: the application lives in the notification area and
        // a closed window cannot be shown again.
        if (!e.Cancel && Application.Current is App { IsExiting: false })
        {
            e.Cancel = true;
            Dismiss();
        }
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        LayoutItems();
        PositionInWorkArea();
    }

    private void LayoutItems()
    {
        var radius = ResourceDouble("Size.RingItemRadius", 123);
        var size = ResourceDouble("Size.ItemBadge", 44);
        var centre = 160.0;

        for (var index = 0; index < ViewModel.Items.Count; index++)
        {
            ViewModel.Items[index].Place(centre, centre, radius, index, ViewModel.Items.Count, size);
        }
    }

    private void BuildItems()
    {
        ViewModel.Items.Clear();
        ViewModel.Items.Add(CreateItem(
            "images",
            "Images",
            "Convert to PNG, JPEG, WEBP, AVIF, BMP, TIFF, GIF or ICO.",
            "Icon.Image",
            "Accent.Magenta"));
        ViewModel.Items.Add(CreateItem(
            "video",
            "Video",
            "Convert to MP4, MKV, MOV, AVI, WEBM or an animated GIF.",
            "Icon.Video",
            "Accent.Violet"));
        ViewModel.Items.Add(CreateItem(
            "audio",
            "Audio",
            "Convert to MP3, WAV, FLAC, AAC, OGG, M4A or WMA.",
            "Icon.Audio",
            "Accent.Teal"));

        // Nothing behind this entry exists yet, so it is disabled and says so rather than
        // pretending to work. It is enabled with the settings window (F-806).
        ViewModel.Items.Add(CreateItem(
            "more",
            "More options",
            "Open the settings, the conversion queue and the history.",
            "Icon.Settings",
            "Text.Secondary",
            "Settings, queue and history are not available yet."));
    }

    private RadialItemViewModel CreateItem(
        string id,
        string label,
        string description,
        string iconKey,
        string accentKey,
        string? disabledReason = null)
    {
        var icon = FindResource(iconKey) as Geometry ?? Geometry.Empty;
        var accent = FindResource(accentKey) as Brush ?? Brushes.Gray;

        return new RadialItemViewModel(id, label, description, icon, accent)
        {
            IsEnabled = disabledReason is null,
            DisabledReason = disabledReason,
        };
    }

    private double ResourceDouble(string key, double fallback) =>
        FindResource(key) is double value ? value : fallback;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                Dismiss();
                return;

            case Key.Home:
                e.Handled = true;
                FocusItem(RingSelection.First(EnabledFlags()));
                return;

            case Key.End:
                e.Handled = true;
                FocusItem(RingSelection.Last(EnabledFlags()));
                return;

            case Key.Enter:
            case Key.Space:
                e.Handled = true;
                ActivateCurrentItem();
                return;

            case Key.Up:
            case Key.Right:
            case Key.Down:
            case Key.Left:
                e.Handled = true;
                MoveFocus(e.Key);
                return;
        }
    }

    private void MoveFocus(Key key)
    {
        var direction = key switch
        {
            Key.Up => RingDirection.Up,
            Key.Right => RingDirection.Right,
            Key.Down => RingDirection.Down,
            _ => RingDirection.Left,
        };

        var current = ViewModel.FocusedItem is null
            ? -1
            : ViewModel.Items.IndexOf(ViewModel.FocusedItem);

        FocusItem(RingSelection.Move(direction, current, EnabledFlags()));
    }

    private void FocusItem(int? index)
    {
        if (index is not int value || value < 0 || value >= ViewModel.Items.Count)
        {
            return;
        }

        FocusItem(ViewModel.Items[value]);
    }

    private void FocusItem(RadialItemViewModel item)
    {
        if (ItemHost.ItemContainerGenerator.ContainerFromItem(item) is not DependencyObject container)
        {
            return;
        }

        FindDescendant<Button>(container)?.Focus();
    }

    private void ActivateCurrentItem()
    {
        var item = ViewModel.FocusedItem ?? ViewModel.HoveredItem;

        if (item is not null)
        {
            _ = ViewModel.Activate(item);
        }
    }

    private bool[] EnabledFlags()
    {
        var flags = new bool[ViewModel.Items.Count];

        for (var index = 0; index < flags.Length; index++)
        {
            flags[index] = ViewModel.Items[index].IsEnabled;
        }

        return flags;
    }

    private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (FindItem(e.NewFocus as DependencyObject) is { } item)
        {
            ViewModel.FocusedItem = item;
        }
    }

    private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (FindItem(e.NewFocus as DependencyObject) is null)
        {
            ViewModel.FocusedItem = null;
        }
    }

    private void OnItemMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: RadialItemViewModel item })
        {
            ViewModel.HoveredItem = item;
        }
    }

    private void OnItemMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: RadialItemViewModel item } &&
            ReferenceEquals(ViewModel.HoveredItem, item))
        {
            ViewModel.HoveredItem = null;
        }
    }

    private void OnItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: RadialItemViewModel item })
        {
            _ = ViewModel.Activate(item);
        }
    }

    /// <summary>
    /// Hides the widget and gives the focus back to the window the user came from.
    /// </summary>
    private void Dismiss()
    {
        ViewModel.HoveredItem = null;
        ViewModel.FocusedItem = null;

        Hide();

        var previous = _previousForeground;
        _previousForeground = IntPtr.Zero;

        if (previous != IntPtr.Zero)
        {
            _ = ForegroundWindowTracker.TryRestore(previous);
        }
    }

    /// <summary>
    /// Returns the radial item a focused element belongs to, if any.
    /// </summary>
    /// <param name="element">The element to inspect.</param>
    /// <returns>The item, or <see langword="null"/> when the element is not a radial badge.</returns>
    private static RadialItemViewModel? FindItem(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is Button { DataContext: RadialItemViewModel item })
            {
                return item;
            }

            // VisualTreeHelper throws for content elements, so the walk stops at anything that is
            // not part of the visual tree.
            element = element is Visual visual ? VisualTreeHelper.GetParent(visual) : null;
        }

        return null;
    }

    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is T match)
        {
            return match;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var found = FindDescendant<T>(VisualTreeHelper.GetChild(root, index));
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
