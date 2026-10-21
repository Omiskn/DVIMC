using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using DiagnosticsProcess = System.Diagnostics.Process;

namespace FluxConvert.Core.Process;

/// <summary>
/// Runs external programs with redirected output, cancellation support and a hard timeout.
/// </summary>
/// <remarks>
/// <para>
/// Arguments are always supplied as a list and handed to the process through
/// <see cref="ProcessStartInfo.ArgumentList"/>. A command line is never assembled by string
/// concatenation and no shell is involved, so a file name containing spaces, quotes, ampersands
/// or non-ASCII characters is passed through unchanged and can never be re-interpreted.
/// </para>
/// <para>
/// Standard output and standard error are drained concurrently, which prevents the classic
/// "process blocks because nobody reads its output pipe" deadlock.
/// </para>
/// </remarks>
public sealed class ProcessRunner : IProcessRunner
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new DiagnosticsProcess { StartInfo = startInfo, EnableRaisingEvents = true };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (!process.Start())
            {
                throw new ExternalToolException(
                    $"'{request.FileName}' could not be started.",
                    request.FileName);
            }
        }
        catch (Win32Exception ex)
        {
            throw new ExternalToolException(
                $"'{request.FileName}' could not be started. The file may be missing or blocked.",
                request.FileName,
                ex);
        }

        // Both streams are drained at the same time; waiting for one before starting the other
        // would deadlock as soon as the process fills the other pipe.
        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutSource = new CancellationTokenSource();
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        if (request.Timeout is { } timeout && timeout > TimeSpan.Zero)
        {
            timeoutSource.CancelAfter(timeout);
        }

        await using var registration = linkedSource.Token.Register(() => TryKill(process)).ConfigureAwait(false);

        try
        {
            await process.WaitForExitAsync(linkedSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // The output tasks must be observed even when the run is abandoned, otherwise their
            // exceptions would surface as unobserved task exceptions later.
            await IgnoreAsync(standardOutputTask).ConfigureAwait(false);
            await IgnoreAsync(standardErrorTask).ConfigureAwait(false);

            stopwatch.Stop();

            if (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"'{request.FileName}' did not finish within {request.Timeout!.Value.TotalSeconds:0.#} seconds.");
            }

            throw;
        }

        stopwatch.Stop();

        var standardOutput = await standardOutputTask.ConfigureAwait(false);
        var standardError = await standardErrorTask.ConfigureAwait(false);

        return new ProcessResult(process.ExitCode, standardOutput, standardError, stopwatch.Elapsed);
    }

    private static void TryKill(DiagnosticsProcess process)
    {        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
            // The process exited between the check and the kill.
        }
        catch (NotSupportedException)
        {
            // The process cannot be killed; nothing else can be done here.
        }
        catch (SystemException)
        {
            // Killing must never mask the original cancellation.
        }
    }

    private static async Task IgnoreAsync(Task<string> task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // The run was abandoned; the partial output is not needed.
        }
        catch (IOException)
        {
            // The pipe was closed while it was being read.
        }
    }
}
