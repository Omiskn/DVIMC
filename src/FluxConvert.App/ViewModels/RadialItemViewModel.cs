using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FluxConvert.Core.Layout;

namespace FluxConvert.App.ViewModels;

/// <summary>
/// One entry on the radial ring.
/// </summary>
public sealed partial class RadialItemViewModel : ObservableObject
{
    /// <summary>Initialises a new instance of the <see cref="RadialItemViewModel"/> class.</summary>
    /// <param name="id">A stable identifier for the item.</param>
    /// <param name="label">The name shown under the item and in the detail strip.</param>
    /// <param name="description">What the item does, shown in the detail strip.</param>
    /// <param name="icon">The glyph of the item.</param>
    /// <param name="accent">The colour of the glyph at rest.</param>
    public RadialItemViewModel(
        string id,
        string label,
        string description,
        Geometry icon,
        Brush accent)
    {
        Id = id;
        Label = label;
        Description = description;
        Icon = icon;
        Accent = accent;
    }

    /// <summary>A stable identifier for the item.</summary>
    public string Id { get; }

    /// <summary>The name of the item.</summary>
    public string Label { get; }

    /// <summary>What the item does.</summary>
    public string Description { get; }

    /// <summary>The glyph of the item.</summary>
    public Geometry Icon { get; }

    /// <summary>The colour of the glyph at rest.</summary>
    public Brush Accent { get; }

    /// <summary>
    /// Why the item cannot be used, or <see langword="null"/> when it can.
    /// </summary>
    /// <remarks>
    /// A disabled option that does not say why it is disabled is a dead end. The reason is shown
    /// in the tooltip, in the detail strip and to a screen reader, so the user is never left
    /// guessing.
    /// </remarks>
    public string? DisabledReason { get; init; }

    /// <summary>The distance of the item centre from the centre of the ring.</summary>
    public double Radius { get; private set; }

    /// <summary>Whether the item can be chosen.</summary>
    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>Whether the item is the current selection.</summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>The left edge of the item inside the ring area.</summary>
    [ObservableProperty]
    private double _left;

    /// <summary>The top edge of the item inside the ring area.</summary>
    [ObservableProperty]
    private double _top;

    /// <summary>
    /// The text shown when the item is hovered or focused.
    /// </summary>
    /// <remarks>
    /// For an available item this is what it does; for an unavailable item it is why it cannot
    /// be used. The same text is bound to the tooltip and to the automation help text.
    /// </remarks>
    public string HelpText => IsEnabled ? Description : DisabledReason ?? Description;

    /// <summary>
    /// Places the item on the ring.
    /// </summary>
    /// <param name="centreX">The horizontal centre of the ring area.</param>
    /// <param name="centreY">The vertical centre of the ring area.</param>
    /// <param name="radius">The distance of the item centres from the centre.</param>
    /// <param name="index">The index of the item.</param>
    /// <param name="count">The number of items on the ring.</param>
    /// <param name="size">The size of the item badge.</param>
    public void Place(double centreX, double centreY, double radius, int index, int count, double size)
    {
        Radius = radius;

        var position = RadialGeometry.GetItemPosition(centreX, centreY, radius, index, count);
        Left = position.X - (size / 2.0);
        Top = position.Y - (size / 2.0);
    }

    partial void OnIsEnabledChanged(bool value) => OnPropertyChanged(nameof(HelpText));
}
