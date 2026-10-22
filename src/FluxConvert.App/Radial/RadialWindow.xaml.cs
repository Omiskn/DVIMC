using System.Windows;
using System.Windows.Input;
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
/// </remarks>
public partial class RadialWindow : Window
{
    private const double WidgetMargin = 16.0;

    /// <summary>Initialises a new instance of the <see cref="RadialWindow"/> class.</summary>
    public RadialWindow()
    {
        InitializeComponent();

        ViewModel = new RadialMenuViewModel();
        DataContext = ViewModel;

        BuildItems();
        Loaded += OnWindowLoaded;
    }

    /// <summary>The state of the radial menu.</summary>
    public RadialMenuViewModel ViewModel { get; }

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
        ViewModel.Items.Add(CreateItem(
            "more",
            "More options",
            "Open the settings, the conversion queue and the history.",
            "Icon.Settings",
            "Text.Secondary"));
    }

    private RadialItemViewModel CreateItem(string id, string label, string description, string iconKey, string accentKey)
    {
        var icon = FindResource(iconKey) as Geometry ?? Geometry.Empty;
        var accent = FindResource(accentKey) as Brush ?? Brushes.Gray;

        return new RadialItemViewModel(id, label, description, icon, accent);
    }

    private double ResourceDouble(string key, double fallback) =>
        FindResource(key) is double value ? value : fallback;

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
}
