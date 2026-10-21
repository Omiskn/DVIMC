namespace FluxConvert.Core.Infrastructure;

/// <summary>
/// Resolves the locations the application uses for its own data.
/// </summary>
/// <remarks>
/// <para>
/// The application data root is, in order of precedence:</para>
/// <list type="number">
///   <item><description>the <c>FLUXCONVERT_DATA_ROOT</c> environment variable (useful for tests
///   and for portable installs);</description></item>
///   <item><description><c>%LOCALAPPDATA%\FluxConvert</c> — the normal location;</description></item>
///   <item><description><c>%APPDATA%\FluxConvert</c> — fallback if the local folder cannot be
///   determined;</description></item>
///   <item><description><c>&lt;executable folder&gt;\data</c> — last resort.</description></item>
/// </list>
/// <para>
/// No path is hard-coded to a specific machine, and no administrator rights are required.
/// </para>
/// </remarks>
public sealed class AppPaths
{
    /// <summary>
    /// Name of the environment variable that overrides the application data root.
    /// </summary>
    public const string RootEnvironmentVariable = "FLUXCONVERT_DATA_ROOT";

    private const string SettingsFileName = "settings.json";
    private const string HistoryFileName = "history.json";
    private const string LogDirectoryName = "logs";
    private const string TempDirectoryName = "temp";

    /// <summary>
    /// Initialises a new instance of the <see cref="AppPaths"/> class.
    /// </summary>
    /// <param name="root">
    /// The application data root. When <see langword="null"/> the root is resolved from the
    /// environment.
    /// </param>
    public AppPaths(string? root = null)
    {
        Root = string.IsNullOrWhiteSpace(root) ? ResolveRootFromEnvironment() : Path.GetFullPath(root);
    }

    /// <summary>The application data root directory.</summary>
    public string Root { get; }

    /// <summary>Full path of the persisted settings file.</summary>
    public string SettingsFile => Path.Combine(Root, SettingsFileName);

    /// <summary>Full path of the persisted conversion history file.</summary>
    public string HistoryFile => Path.Combine(Root, HistoryFileName);

    /// <summary>Directory that holds the rotating log files.</summary>
    public string LogDirectory => Path.Combine(Root, LogDirectoryName);

    /// <summary>Directory used for in-progress temporary conversion output.</summary>
    public string TempDirectory => Path.Combine(Root, TempDirectoryName);

    /// <summary>
    /// Creates the application data directories if they do not exist yet.
    /// </summary>
    /// <returns>The application data root.</returns>
    /// <exception cref="IOException">The directories could not be created.</exception>
    /// <exception cref="UnauthorizedAccessException">The directories could not be created.</exception>
    public string EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(LogDirectory);
        Directory.CreateDirectory(TempDirectory);
        return Root;
    }

    private static string ResolveRootFromEnvironment()
    {
        var overridden = Environment.GetEnvironmentVariable(RootEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overridden))
        {
            return Path.GetFullPath(overridden);
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            return Path.Combine(localAppData, ProductInfo.ProductName);
        }

        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(roamingAppData))
        {
            return Path.Combine(roamingAppData, ProductInfo.ProductName);
        }

        return Path.Combine(AppContext.BaseDirectory, "data");
    }
}
