using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FluxConvert.Core.Infrastructure;

/// <summary>
/// A logger provider that writes structured log lines to a daily rotating file.
/// </summary>
/// <remarks>
/// <para>
/// One file per day is created in the supplied directory, named
/// <c>fluxconvert-YYYYMMDD.log</c>. Lines are appended and flushed immediately so that a crash
/// cannot lose the most recent entries.
/// </para>
/// <para>
/// The file is opened for the duration of a single write and closed again immediately. The
/// application therefore never holds a lock on its own log, so the file can be opened in Notepad,
/// copied or tailed while the application is running.
/// </para>
/// <para>
/// The provider is deliberately small: it only needs to record what happened, when, and in which
/// category. It never writes file contents or anything else that could be sensitive.
/// </para>
/// </remarks>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly string _logDirectory;
    private readonly LogLevel _minimumLevel;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>
    /// Initialises a new instance of the <see cref="FileLoggerProvider"/> class.
    /// </summary>
    /// <param name="logDirectory">Directory the log files are written to.</param>
    /// <param name="minimumLevel">The lowest level that is written.</param>
    /// <param name="clock">Clock used for the file name and the timestamps. Defaults to the
    /// system clock.</param>
    public FileLoggerProvider(
        string logDirectory,
        LogLevel minimumLevel = LogLevel.Information,
        Func<DateTimeOffset>? clock = null)
    {
        _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
        _minimumLevel = minimumLevel;
        _clock = clock ?? (() => DateTimeOffset.Now);
    }

    /// <summary>
    /// Builds the file name used for a given day.
    /// </summary>
    /// <param name="timestamp">The timestamp whose date is used.</param>
    /// <returns>A file name of the form <c>fluxconvert-YYYYMMDD.log</c>.</returns>
    public static string BuildFileName(DateTimeOffset timestamp) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"fluxconvert-{timestamp:yyyyMMdd}.log");

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= _minimumLevel;

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing is held open between writes.
    }

    private void Write(LogLevel level, string category, EventId eventId, string message, Exception? exception)
    {
        if (!IsEnabled(level))
        {
            return;
        }

        var timestamp = _clock();
        var builder = new StringBuilder(256);
        builder.Append(timestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture));
        builder.Append(" [").Append(Abbreviate(level)).Append("] ");
        builder.Append(category);

        if (eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name))
        {
            builder.Append('/').Append(eventId.Name ?? eventId.Id.ToString(CultureInfo.InvariantCulture));
        }

        builder.Append(": ").Append(message);

        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        try
        {
            Directory.CreateDirectory(_logDirectory);
            var path = Path.Combine(_logDirectory, BuildFileName(timestamp));

            using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream, FileEncoding);
            writer.WriteLine(builder.ToString());
            writer.Flush();
        }
        catch (IOException)
        {
            // Logging must never take the application down.
        }
        catch (UnauthorizedAccessException)
        {
            // Logging must never take the application down.
        }
    }

    private static string Abbreviate(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "OFF",
    };

    private sealed class FileLogger : ILogger
    {
        private readonly FileLoggerProvider _provider;
        private readonly string _category;

        public FileLogger(FileLoggerProvider provider, string category)
        {
            _provider = provider;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NoopScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter is null ? state?.ToString() ?? string.Empty : formatter(state, exception);
            _provider.Write(logLevel, _category, eventId, message, exception);
        }
    }

    /// <summary>
    /// Scope object used when the caller does not provide one. Logging scopes are not supported by
    /// the file provider, so this exists purely to satisfy the interface.
    /// </summary>
    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();

        private NoopScope()
        {
        }

        public void Dispose()
        {
            // Nothing to release.
        }
    }
}
