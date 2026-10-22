using System.Windows;

namespace FluxConvert.App.Controls;

/// <summary>
/// Attached properties used by the radial item badges.
/// </summary>
public static class RadialItem
{
    /// <summary>
    /// Whether the item is the currently selected entry of the radial menu.
    /// </summary>
    /// <remarks>
    /// A plain <see cref="System.Windows.Controls.Button"/> has no selected state, so the
    /// template triggers need one of their own. Keeping it as an attached property means the
    /// badge itself stays a normal, fully accessible button.
    /// </remarks>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(RadialItem),
            new PropertyMetadata(false));

    /// <summary>Gets whether the item is selected.</summary>
    /// <param name="element">The element to read from.</param>
    /// <returns><see langword="true"/> when the item is selected.</returns>
    public static bool GetIsSelected(DependencyObject element) =>
        (bool)element.GetValue(IsSelectedProperty);

    /// <summary>Sets whether the item is selected.</summary>
    /// <param name="element">The element to write to.</param>
    /// <param name="value">The new value.</param>
    public static void SetIsSelected(DependencyObject element, bool value) =>
        element.SetValue(IsSelectedProperty, value);
}
