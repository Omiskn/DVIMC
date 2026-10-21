using System.Text.Json;
using System.Text.Json.Serialization;
using FluxConvert.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FluxConvert.Core.Settings;

/// <summary>
/// Persists <see cref="AppSettings"/> as JSON in the application data directory.
/// </summary>
/// <remarks>
/// <para>
/// Writes are atomic: the new content is written to a temporary file next to the target and then
/// moved over it, so an interrupted write can never leave a half-written settings file behind.
/// </para>
/// <para>
/// A file that cannot be parsed is moved aside as <c>settings.corrupt-&lt;timestamp&gt;.json</c>
/// and the defaults are used, so a bad file never prevents the application from starting.
/// </para>
/// </remarks>
public sealed partial class SettingsService : ISettingsService, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly AppPaths _paths;
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="paths">Provides the location of the settings file.</param>
    /// <param name="logger">Logger used to report problems.</param>
    public SettingsService(AppPaths paths, ILogger<SettingsService> logger)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Current = new AppSettings();
    }

    /// <inheritdoc />
    public AppSettings Current { get; private set; }

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public void Load()
    {
        var path = _paths.SettingsFile;

        if (!File.Exists(path))
        {
            LogNoSettingsFile(path);
            Current = new AppSettings();
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
            Current = loaded ?? new AppSettings();
            LogSettingsLoaded(path);
        }
        catch (JsonException ex)
        {
            LogSettingsCorrupt(ex, path);
            Quarantine(path);
            Current = new AppSettings();
        }
        catch (IOException ex)
        {
            LogSettingsUnreadable(ex, path);
            Current = new AppSettings();
        }
        catch (UnauthorizedAccessException ex)
        {
            LogSettingsUnreadable(ex, path);
            Current = new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var path = _paths.SettingsFile;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Current, SerializerOptions);
            var temporaryPath = path + ".tmp";

            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, path, overwrite: true);

            LogSettingsSaved(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LogSettingsNotSaved(ex, _paths.SettingsFile);
        }
        finally
        {
            _writeLock.Release();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _writeLock.Dispose();
    }

    private void Quarantine(string path)
    {
        try
        {
            var backup = Path.Combine(
                Path.GetDirectoryName(path) ?? string.Empty,
                $"settings.corrupt-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.json");
            File.Move(path, backup, overwrite: true);
            LogSettingsQuarantined(backup);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LogQuarantineFailed(ex, path);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "No settings file found at {Path}; using defaults.")]
    private partial void LogNoSettingsFile(string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Settings loaded from {Path}.")]
    private partial void LogSettingsLoaded(string path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Settings file {Path} is not valid JSON; keeping defaults.")]
    private partial void LogSettingsCorrupt(Exception exception, string path);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Settings file {Path} could not be read; keeping defaults.")]
    private partial void LogSettingsUnreadable(Exception exception, string path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Settings saved to {Path}.")]
    private partial void LogSettingsSaved(string path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Settings could not be saved to {Path}.")]
    private partial void LogSettingsNotSaved(Exception exception, string path);

    [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "Moved unreadable settings file to {Backup}.")]
    private partial void LogSettingsQuarantined(string backup);

    [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "The unreadable settings file {Path} could not be moved aside.")]
    private partial void LogQuarantineFailed(Exception exception, string path);
}
