using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluxConvert.App.ViewModels;

/// <summary>
/// The state of the radial menu.
/// </summary>
/// <remarks>
/// Three things can describe the current item, in order of precedence: the pointer hovering it,
/// the keyboard focus being on it, and it being the selected item. The detail strip always
/// describes the highest one that is set, so the strip never goes blank while the user is
/// pointing at something.
/// </remarks>
public sealed partial class RadialMenuViewModel : ObservableObject
{
    /// <summary>The items on the ring.</summary>
    public ObservableCollection<RadialItemViewModel> Items { get; } = new();

    /// <summary>The name shown in the core disc.</summary>
    [ObservableProperty]
    private string _title = "FluxConvert";

    /// <summary>The line shown under the name in the core disc.</summary>
    [ObservableProperty]
    private string _subtitle = "Drop a file to convert";

    /// <summary>The item the pointer is currently over.</summary>
    [ObservableProperty]
    private RadialItemViewModel? _hoveredItem;

    /// <summary>The item the keyboard is currently on.</summary>
    [ObservableProperty]
    private RadialItemViewModel? _focusedItem;

    /// <summary>The item the user has chosen.</summary>
    [ObservableProperty]
    private RadialItemViewModel? _selectedItem;

    /// <summary>The item the detail strip describes.</summary>
    public RadialItemViewModel? ActiveItem => HoveredItem ?? FocusedItem ?? SelectedItem;

    /// <summary>The title of the detail strip.</summary>
    public string DetailTitle
    {
        get
        {
            var item = ActiveItem;

            if (item is null)
            {
                return "Ready";
            }

            return item.IsSelected ? item.Label + " · selected" : item.Label;
        }
    }

    /// <summary>The description of the detail strip.</summary>
    public string DetailDescription =>
        ActiveItem?.HelpText ?? "Pick a category to choose an output format.";

    /// <summary>Whether the detail strip is describing an item that cannot be used.</summary>
    public bool DetailIsWarning => ActiveItem is { IsEnabled: false };

    /// <summary>
    /// Selects an item.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <returns>
    /// <see langword="true"/> when the item was selected; <see langword="false"/> when it is
    /// disabled and therefore cannot be chosen.
    /// </returns>
    public bool Activate(RadialItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!item.IsEnabled)
        {
            return false;
        }

        SelectedItem = item;
        return true;
    }

    partial void OnHoveredItemChanged(RadialItemViewModel? value) => OnActiveItemChanged();

    partial void OnFocusedItemChanged(RadialItemViewModel? value) => OnActiveItemChanged();

    partial void OnSelectedItemChanged(RadialItemViewModel? oldValue, RadialItemViewModel? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.IsSelected = false;
        }

        if (newValue is not null)
        {
            newValue.IsSelected = true;
        }

        OnActiveItemChanged();
    }

    private void OnActiveItemChanged()
    {
        OnPropertyChanged(nameof(ActiveItem));
        OnPropertyChanged(nameof(DetailTitle));
        OnPropertyChanged(nameof(DetailDescription));
        OnPropertyChanged(nameof(DetailIsWarning));
    }
}
