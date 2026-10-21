namespace FluxConvert.Core.Shell;

/// <summary>
/// Tracks whether any conversion is currently running.
/// </summary>
/// <remarks>
/// The application shell uses this to decide whether quitting needs a confirmation. The
/// conversion queue registers itself here when it is built; until then the count is always zero
/// and quitting is silent.
/// </remarks>
public interface IConversionActivity
{
    /// <summary>The number of conversions that are running right now.</summary>
    int ActiveCount { get; }

    /// <summary>Whether at least one conversion is running.</summary>
    bool IsBusy { get; }

    /// <summary>
    /// Registers a running conversion.
    /// </summary>
    /// <returns>A scope that marks the conversion as finished when it is disposed.</returns>
    IDisposable Track();
}
