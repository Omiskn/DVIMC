namespace FluxConvert.Core.Shell;

/// <summary>
/// Thread-safe <see cref="IConversionActivity"/> implementation.
/// </summary>
public sealed class ConversionActivityTracker : IConversionActivity
{
    private int _activeCount;

    /// <inheritdoc />
    public int ActiveCount => Volatile.Read(ref _activeCount);

    /// <inheritdoc />
    public bool IsBusy => ActiveCount > 0;

    /// <inheritdoc />
    public IDisposable Track()
    {
        Interlocked.Increment(ref _activeCount);
        return new Scope(this);
    }

    private void Release() => Interlocked.Decrement(ref _activeCount);

    private sealed class Scope : IDisposable
    {
        private ConversionActivityTracker? _owner;

        public Scope(ConversionActivityTracker owner) => _owner = owner;

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Release();
        }
    }
}
