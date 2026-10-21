namespace FluxConvert.Core.Process;

/// <summary>
/// Runs external programs asynchronously.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a program and waits for it to finish.
    /// </summary>
    /// <param name="request">The program and its arguments.</param>
    /// <param name="cancellationToken">Token used to cancel the run.</param>
    /// <returns>The exit code and the captured output.</returns>
    /// <exception cref="ExternalToolException">The program could not be started.</exception>
    /// <exception cref="TimeoutException">The program did not finish within the requested time.</exception>
    /// <exception cref="OperationCanceledException">The run was cancelled.</exception>
    Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default);
}
