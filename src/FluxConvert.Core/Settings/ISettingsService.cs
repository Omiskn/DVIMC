namespace FluxConvert.Core.Settings;

/// <summary>
/// Reads and writes the persisted user preferences.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// The settings currently in effect. Always non-null; before <see cref="Load"/> is called it
    /// contains the defaults.
    /// </summary>
    AppSettings Current { get; }

    /// <summary>Raised after the settings have been changed and persisted.</summary>
    event EventHandler? Changed;

    /// <summary>
    /// Loads the settings from disk. Missing or unreadable files leave the defaults in place.
    /// </summary>
    /// <remarks>
    /// This is a synchronous call on purpose: it reads a single small local file and must
    /// complete before the interface shows any preference to the user.
    /// </remarks>
    void Load();

    /// <summary>
    /// Persists the current settings to disk.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the write.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
