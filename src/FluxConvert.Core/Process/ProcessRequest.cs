namespace FluxConvert.Core.Process;

/// <summary>
/// A request to run an external program.
/// </summary>
/// <param name="FileName">Full path of the executable. Never a shell command line.</param>
/// <param name="Arguments">
/// The arguments, each one a separate entry. They are passed to the process directly, so a path
/// containing spaces, quotes or non-ASCII characters is never re-parsed by a shell.
/// </param>
/// <param name="WorkingDirectory">Optional working directory for the process.</param>
/// <param name="Timeout">Optional time limit after which the process is terminated.</param>
public sealed record ProcessRequest(
    string FileName,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory = null,
    TimeSpan? Timeout = null);
