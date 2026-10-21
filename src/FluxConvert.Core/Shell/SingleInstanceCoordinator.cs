namespace FluxConvert.Core.Shell;

/// <summary>
/// Makes sure only one copy of the application runs, and lets a second copy ask the first one to
/// show its interface.
/// </summary>
/// <remarks>
/// <para>
/// The first instance claims a named mutex; every later instance finds the mutex taken and
/// therefore knows it is a secondary copy. A secondary copy signals a named auto-reset event,
/// which the primary instance is waiting on, and then exits.
/// </para>
/// <para>
/// The names are prefixed with <c>Local\</c> so the coordination stays inside the current
/// Windows session and never needs elevated rights.
/// </para>
/// </remarks>
public sealed class SingleInstanceCoordinator : IDisposable
{
    /// <summary>The default application name used to build the system object names.</summary>
    public const string DefaultApplicationName = "FluxConvert";

    private static readonly HashSet<string> ProcessClaims = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object ProcessClaimsLock = new();

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _activationEvent;
    private readonly string _applicationName;
    private bool _ownsMutex;
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="SingleInstanceCoordinator"/> class and
    /// attempts to become the primary instance.
    /// </summary>
    /// <param name="applicationName">Name used to build the system object names.</param>
    public SingleInstanceCoordinator(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("An application name is required.", nameof(applicationName));
        }

        _applicationName = applicationName;
        _mutex = new Mutex(false, BuildMutexName(applicationName));
        _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, BuildEventName(applicationName));
        _ownsMutex = TryAcquireMutex() && TryClaimProcess(applicationName);

        if (!_ownsMutex)
        {
            // A named mutex is re-entrant for the thread that already owns it, so the mutex alone
            // cannot tell a second coordinator in the same process apart from a second process.
            // The process-wide claim closes that gap.
            ReleaseMutexQuietly();
        }
    }

    /// <summary>
    /// Whether this process is the primary instance and therefore owns the application.
    /// </summary>
    public bool IsPrimary => _ownsMutex;

    /// <summary>
    /// Starts listening for activation requests coming from a secondary instance.
    /// </summary>
    /// <param name="onActivated">
    /// Called on a background thread each time another instance asks the application to show
    /// itself. The caller is responsible for moving the work to the user interface thread.
    /// </param>
    /// <returns>A handle that stops the listener when it is disposed.</returns>
    public IDisposable StartActivationListener(Action onActivated)
    {
        ArgumentNullException.ThrowIfNull(onActivated);

        var registration = ThreadPool.RegisterWaitForSingleObject(
            _activationEvent,
            (_, timedOut) =>
            {
                if (timedOut)
                {
                    return;
                }

                try
                {
                    onActivated();
                }
                catch (Exception)
                {
                    // A failing listener must not take the process down.
                }
            },
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);

        return new Registration(registration);
    }

    /// <summary>
    /// Asks the primary instance to show its interface.
    /// </summary>
    /// <param name="applicationName">Name used to build the system object names.</param>
    /// <returns><see langword="true"/> when the signal was delivered.</returns>
    public static bool SignalPrimary(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName) || !OperatingSystem.IsWindows())
        {
            // Named system objects are a Windows facility. The application only ships for
            // Windows, but the library stays portable and simply reports "no primary instance".
            return false;
        }

        try
        {
            using var handle = EventWaitHandle.OpenExisting(BuildEventName(applicationName));
            handle.Set();
            return true;
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // No primary instance is running.
            return false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_ownsMutex)
        {
            ReleaseMutexQuietly();
            ReleaseProcessClaim(_applicationName);
            _ownsMutex = false;
        }

        _activationEvent.Dispose();
        _mutex.Dispose();
    }

    /// <summary>The system object name of the activation event.</summary>
    public static string BuildEventName(string applicationName) => $@"Local\{applicationName}.Activate";

    /// <summary>The system object name of the single-instance mutex.</summary>
    public static string BuildMutexName(string applicationName) => $@"Local\{applicationName}.SingleInstance";

    private bool TryAcquireMutex()
    {
        try
        {
            return _mutex.WaitOne(0, false);
        }
        catch (AbandonedMutexException)
        {
            // The previous owner died without releasing the mutex. This thread now owns it.
            return true;
        }
    }

    private void ReleaseMutexQuietly()
    {
        try
        {
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException)
        {
            // The mutex was not owned by this thread; there is nothing to release.
        }
        catch (ObjectDisposedException)
        {
            // The handle was already closed.
        }
    }

    private static bool TryClaimProcess(string applicationName)
    {
        lock (ProcessClaimsLock)
        {
            if (ProcessClaims.Contains(applicationName))
            {
                return false;
            }

            ProcessClaims.Add(applicationName);
            return true;
        }
    }

    private static void ReleaseProcessClaim(string applicationName)
    {
        lock (ProcessClaimsLock)
        {
            ProcessClaims.Remove(applicationName);
        }
    }

    private sealed class Registration : IDisposable
    {
        private RegisteredWaitHandle? _handle;

        public Registration(RegisteredWaitHandle handle) => _handle = handle;

        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, null);
            handle?.Unregister(null);
        }
    }
}
