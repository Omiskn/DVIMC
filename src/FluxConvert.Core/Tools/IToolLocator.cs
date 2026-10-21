namespace FluxConvert.Core.Tools;

/// <summary>
/// Finds the external programs the application relies on.
/// </summary>
public interface IToolLocator
{
    /// <summary>
    /// Locates a program, caching the result.
    /// </summary>
    /// <param name="kind">The program to look for.</param>
    /// <param name="cancellationToken">Token used to cancel the version probe.</param>
    /// <returns>The result of the search. A program that cannot be found is reported as
    /// unavailable, never as an exception.</returns>
    Task<ExternalTool> GetAsync(ExternalToolKind kind, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards the cached results so the next lookup runs again.
    /// </summary>
    void Invalidate();
}
