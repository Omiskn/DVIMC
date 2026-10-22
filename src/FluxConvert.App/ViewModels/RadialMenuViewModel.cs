using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluxConvert.App.ViewModels;

/// <summary>
/// The state of the radial menu.
/// </summary>
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

    /// <summary>The title of the detail strip.</summary>
    public string DetailTitle => HoveredItem?.Label ?? "Ready";

    /// <summary>The description of the detail strip.</summary>
    public string DetailDescription =>
        HoveredItem?.Description ?? "Pick a category to choose an output format.";

    partial void OnHoveredItemChanged(RadialItemViewModel? value)
    {
        OnPropertyChanged(nameof(DetailTitle));
        OnPropertyChanged(nameof(DetailDescription));
    }
}
