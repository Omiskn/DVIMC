using System.Diagnostics;
using FluxConvert.Core.Process;
using Xunit;

namespace FluxConvert.Core.Tests;

/// <summary>
/// Tests for <see cref="ProcessRunner"/> that really start a process.
/// </summary>
/// <remarks>
/// The tests use <c>cmd.exe</c> because it is present on every supported Windows version. They
/// exercise the parts that matter for the media tools: argument handling, output capture, exit
/// codes, timeouts and cancellation.
/// </remarks>
public class ProcessRunnerTests
{
    private static readonly ProcessRunner Runner = new();

    private static string CommandShell =>
        Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";

    [Fact]
    public async Task CapturesStandardOutput()
    {
        var result = await Runner.RunAsync(new ProcessRequest(CommandShell, new[] { "/c", "echo", "hello" }));

        Assert.True(result.Succeeded);
        Assert.Contains("hello", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReturnsANonZeroExitCode()
    {
        var result = await Runner.RunAsync(new ProcessRequest(CommandShell, new[] { "/c", "exit", "3" }));

        Assert.Equal(3, result.ExitCode);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task PassesAnArgumentWithSpacesAsASingleArgument()
    {
        var result = await Runner.RunAsync(new ProcessRequest(CommandShell, new[] { "/c", "echo", "a b c" }));

        Assert.Contains("a b c", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DoesNotLetShellCharactersEscapeTheirArgument()
    {
        // If the argument were re-parsed by a shell, the "&" would start a second command.
        var result = await Runner.RunAsync(new ProcessRequest(CommandShell, new[] { "/c", "echo", "x & y" }));

        Assert.Contains("x & y", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("& y", result.StandardOutput.Replace("x & y", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReportsAMissingExecutable()
    {
        var missing = Path.Combine(Path.GetTempPath(), "fluxconvert-missing-" + Guid.NewGuid().ToString("N") + ".exe");

        await Assert.ThrowsAsync<ExternalToolException>(
            () => Runner.RunAsync(new ProcessRequest(missing, new[] { "-version" })));
    }

    [Fact]
    public async Task ThrowsWhenTheTimeoutIsExceeded()
    {
        var request = new ProcessRequest(
            CommandShell,
            new[] { "/c", "ping", "-n", "30", "127.0.0.1" },
            Timeout: TimeSpan.FromMilliseconds(800));

        await Assert.ThrowsAsync<TimeoutException>(() => Runner.RunAsync(request));
    }

    [Fact]
    public async Task ThrowsWhenTheRunIsCancelled()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        var request = new ProcessRequest(CommandShell, new[] { "/c", "ping", "-n", "30", "127.0.0.1" });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Runner.RunAsync(request, cancellation.Token));
    }

    [Fact]
    public async Task LeavesNoChildProcessBehindAfterCancellation()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        var request = new ProcessRequest(CommandShell, new[] { "/c", "ping", "-n", "30", "127.0.0.1" });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Runner.RunAsync(request, cancellation.Token));

        // Give the operating system a moment to reap the killed process tree.
        await Task.Delay(300);

        var running = System.Diagnostics.Process.GetProcessesByName("ping");
        try
        {
            Assert.Empty(running);
        }
        finally
        {
            foreach (var process in running)
            {
                process.Dispose();
            }
        }
    }
}
