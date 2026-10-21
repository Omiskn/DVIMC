using System.IO;
using System.Runtime.InteropServices;

namespace FluxConvert.App.Diagnostics;

/// <summary>
/// Gives a windowed application access to a console so it can print command-line output.
/// </summary>
/// <remarks>
/// The application is a <c>WinExe</c>, so it has no console of its own. When it is started from
/// a terminal the parent process owns the console; attaching to it makes standard output work
/// again. When there is no console at all, one is allocated so the output stays visible.
/// </remarks>
internal static class ConsoleOutput
{
    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    /// <summary>
    /// Whether a console had to be created because the process had none.
    /// </summary>
    public static bool CreatedConsole { get; private set; }

    /// <summary>
    /// Makes sure the process can write to a console.
    /// </summary>
    /// <returns><see langword="true"/> when output will be visible.</returns>
    public static bool Ensure()
    {
        // Standard output that is already redirected to a file or a pipe must not be replaced:
        // the caller explicitly asked for the output to end up there. Re-opening the handle
        // after attaching to a console would silently send it somewhere else.
        if (Console.IsOutputRedirected)
        {
            return true;
        }

        if (GetConsoleWindow() != IntPtr.Zero)
        {
            // The process already has a console, for example when it was started from a terminal.
            return true;
        }

        // The parent process owns the console when the application was started from a terminal.
        if (!AttachConsole(AttachParentProcess))
        {
            if (!AllocConsole())
            {
                return false;
            }

            CreatedConsole = true;
        }

        ReopenStandardStreams();
        return true;
    }

    private static void ReopenStandardStreams()
    {
        try
        {
            var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            var stderr = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetOut(stdout);
            Console.SetError(stderr);
        }
        catch (IOException)
        {
            // There is no usable standard output; the caller decides how to report that.
        }
        catch (PlatformNotSupportedException)
        {
            // The platform does not support standard streams.
        }
    }

    /// <summary>
    /// Waits for the user to acknowledge the output, but only when the console was created by
    /// this process and would otherwise disappear together with it.
    /// </summary>
    public static void WaitForAcknowledgement()
    {
        if (!CreatedConsole)
        {
            return;
        }

        try
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter to close this window.");
            Console.ReadLine();
        }
        catch (IOException)
        {
            // Nothing to wait for.
        }
    }
}
