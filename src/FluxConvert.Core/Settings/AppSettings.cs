using QualityPresetKind = FluxConvert.Core.Settings.QualityPreset;

namespace FluxConvert.Core.Settings;

/// <summary>
/// Quality presets shared by the image, audio and video pipelines.
/// </summary>
public enum QualityPreset
{
    /// <summary>Fastest encode, smallest file, lowest fidelity.</summary>
    Fast,

    /// <summary>Default balance of speed and quality.</summary>
    Balanced,

    /// <summary>Highest fidelity, slowest encode.</summary>
    HighQuality,

    /// <summary>User-supplied values are used instead of a preset.</summary>
    Custom,
}

/// <summary>
/// User preferences persisted between sessions.
/// </summary>
/// <remarks>
/// The shape of this class is part of the on-disk contract: it is serialised to
/// <c>settings.json</c> with <see cref="System.Text.Json"/>. Unknown members are ignored when
/// reading so that a newer file never breaks an older build, and every property has a sensible
/// default so a missing or partially written file is never fatal.
/// </remarks>
public sealed class AppSettings
{
    /// <summary>Default destination folder used when the user has not chosen one.</summary>
    public const string DefaultQualityPresetName = nameof(QualityPresetKind.Balanced);

    private const int MinConcurrency = 1;
    private const int MaxConcurrency = 8;

    private int _maxConcurrentConversions = 2;
    private string _qualityPreset = DefaultQualityPresetName;
    private string _defaultOutputDirectory = string.Empty;
    private string _theme = "Dark";
    private bool _stripMetadata = true;
    private bool _startMinimizedToTray;
    private bool _openOutputFolderOnCompletion;
    private bool _reducedMotion;
    private bool _preserveSourceFolderStructure;
    private bool _clearCompletedJobsFromHistory = true;
    private string _ffmpegPath = string.Empty;
    private string _imageMagickPath = string.Empty;
    private string[] _favoriteFormats = Array.Empty<string>();
    private string[] _recentFormats = Array.Empty<string>();

    /// <summary>Destination folder for converted files. Empty means "next to the source file".</summary>
    public string DefaultOutputDirectory
    {
        get => _defaultOutputDirectory;
        set => _defaultOutputDirectory = value ?? string.Empty;
    }

    /// <summary>Default quality preset name.</summary>
    public string QualityPreset
    {
        get => _qualityPreset;
        set => _qualityPreset = string.IsNullOrWhiteSpace(value)
            ? DefaultQualityPresetName
            : value.Trim();
    }

    /// <summary>Maximum number of conversions running at the same time.</summary>
    public int MaxConcurrentConversions
    {
        get => _maxConcurrentConversions;
        set => _maxConcurrentConversions = Math.Clamp(value, MinConcurrency, MaxConcurrency);
    }

    /// <summary>Application theme name. Only <c>Dark</c> is implemented so far.</summary>
    public string Theme
    {
        get => _theme;
        set => _theme = string.IsNullOrWhiteSpace(value) ? "Dark" : value.Trim();
    }

    /// <summary>Whether source metadata (EXIF and friends) is removed by default.</summary>
    public bool StripMetadata
    {
        get => _stripMetadata;
        set => _stripMetadata = value;
    }

    /// <summary>
    /// Whether the application starts hidden in the notification area. Defaults to <see langword="false"/>
    /// while the placeholder shell is the only interface; it becomes the default once the floating
    /// radial widget exists.
    /// </summary>
    public bool StartMinimizedToTray
    {
        get => _startMinimizedToTray;
        set => _startMinimizedToTray = value;
    }

    /// <summary>Whether the output folder is opened automatically when a conversion finishes.</summary>
    public bool OpenOutputFolderOnCompletion
    {
        get => _openOutputFolderOnCompletion;
        set => _openOutputFolderOnCompletion = value;
    }

    /// <summary>Whether animations are suppressed.</summary>
    public bool ReducedMotion
    {
        get => _reducedMotion;
        set => _reducedMotion = value;
    }

    /// <summary>Whether batch conversions mirror the source folder structure.</summary>
    public bool PreserveSourceFolderStructure
    {
        get => _preserveSourceFolderStructure;
        set => _preserveSourceFolderStructure = value;
    }

    /// <summary>Whether completed entries are removed from the history automatically.</summary>
    public bool ClearCompletedJobsFromHistory
    {
        get => _clearCompletedJobsFromHistory;
        set => _clearCompletedJobsFromHistory = value;
    }

    /// <summary>Explicit path to <c>ffmpeg.exe</c>. Empty means "discover it".</summary>
    public string FfmpegPath
    {
        get => _ffmpegPath;
        set => _ffmpegPath = value ?? string.Empty;
    }

    /// <summary>Explicit path to <c>magick.exe</c>. Empty means "discover it".</summary>
    public string ImageMagickPath
    {
        get => _imageMagickPath;
        set => _imageMagickPath = value ?? string.Empty;
    }

    /// <summary>Output formats the user pinned to the top of the format list.</summary>
    public string[] FavoriteFormats
    {
        get => _favoriteFormats;
        set => _favoriteFormats = value ?? Array.Empty<string>();
    }

    /// <summary>Most recently used output formats, most recent first.</summary>
    public string[] RecentFormats
    {
        get => _recentFormats;
        set => _recentFormats = value ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a deep copy of these settings.
    /// </summary>
    /// <returns>An independent copy.</returns>
    public AppSettings Clone() => new()
    {
        DefaultOutputDirectory = DefaultOutputDirectory,
        QualityPreset = QualityPreset,
        MaxConcurrentConversions = MaxConcurrentConversions,
        Theme = Theme,
        StripMetadata = StripMetadata,
        StartMinimizedToTray = StartMinimizedToTray,
        OpenOutputFolderOnCompletion = OpenOutputFolderOnCompletion,
        ReducedMotion = ReducedMotion,
        PreserveSourceFolderStructure = PreserveSourceFolderStructure,
        ClearCompletedJobsFromHistory = ClearCompletedJobsFromHistory,
        FfmpegPath = FfmpegPath,
        ImageMagickPath = ImageMagickPath,
        FavoriteFormats = (string[])FavoriteFormats.Clone(),
        RecentFormats = (string[])RecentFormats.Clone(),
    };
}
