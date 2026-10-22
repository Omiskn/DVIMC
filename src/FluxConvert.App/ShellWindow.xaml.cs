using System.ComponentModel;
using System.Windows;
using FluxConvert.App.Converters;

namespace FluxConvert.App;

/// <summary>
/// Temporary application shell window. Replaced by the floating radial widget in Phase 2.
/// </summary>
/// <remarks>
/// While the radial widget is being built, the shell doubles as the design-system preview so
/// that every token, style and icon can be looked at in the running application.
/// </remarks>
public partial class ShellWindow : Window
{
    /// <summary>Initialises the window.</summary>
    public ShellWindow()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Show the keyboard focus state in the preview. Focusing also scrolls the badge into
        // view, which is where the interactive states are shown.
        FocusExample.Focus();
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

    /// <summary>The colour tokens that the preview shows.</summary>
    public IReadOnlyList<Swatch> Swatches { get; } = new[]
    {
        new Swatch("Surface.Base", "#0B0F1A"),
        new Swatch("Surface.Elevated", "#141A2B"),
        new Swatch("Surface.Ring", "#1B2236"),
        new Swatch("Surface.Item", "#232C44"),
        new Swatch("Surface.ItemHover", "#2E3A5C"),
        new Swatch("Border.Subtle", "#2A3350"),
        new Swatch("Text.Primary", "#E8ECF7"),
        new Swatch("Text.Secondary", "#9AA6C4"),
        new Swatch("Text.Muted", "#66739A"),
        new Swatch("Accent.Blue", "#4C8DFF"),
        new Swatch("Accent.Violet", "#8B6CF0"),
        new Swatch("Accent.Teal", "#2DD4BF"),
        new Swatch("Accent.Magenta", "#F472B6"),
        new Swatch("State.Success", "#34D399"),
        new Swatch("State.Warning", "#FBBF24"),
        new Swatch("State.Error", "#F87171"),
    };

    /// <summary>The icons that the preview shows.</summary>
    public IReadOnlyList<IconEntry> Icons { get; } = new[]
    {
        new IconEntry("Image", "Icon.Image"),
        new IconEntry("Video", "Icon.Video"),
        new IconEntry("Audio", "Icon.Audio"),
        new IconEntry("Settings", "Icon.Settings"),
        new IconEntry("Chevron", "Icon.ChevronRight"),
        new IconEntry("Close", "Icon.Close"),
        new IconEntry("Check", "Icon.Check"),
        new IconEntry("Folder", "Icon.Folder"),
        new IconEntry("History", "Icon.History"),
        new IconEntry("Queue", "Icon.Queue"),
        new IconEntry("Retry", "Icon.Retry"),
        new IconEntry("Warning", "Icon.Warning"),
        new IconEntry("Error", "Icon.Error"),
        new IconEntry("Search", "Icon.Search"),
    };
}
