namespace FluxConvert.Core.Process;

/// <summary>
/// The outcome of running an external program.
/// </summary>
/// <param name="ExitCode">The process exit code.</param>
/// <param name="StandardOutput">Everything the process wrote to standard output.</param>
/// <param name="StandardError">Everything the process wrote to standard error.</param>
/// <param name="Duration">How long the process ran.</param>
public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    TimeSpan Duration)
{
    /// <summary>Whether the process reported success.</summary>
    public bool Succeeded => ExitCode == 0;
}
