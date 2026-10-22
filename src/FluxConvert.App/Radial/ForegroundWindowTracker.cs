using System.Runtime.InteropServices;

namespace FluxConvert.App.Radial;

/// <summary>
/// Remembers which window was in front before the widget took the focus, and gives the focus back.
/// </summary>
/// <remarks>
/// <para>
/// The floating interface is summoned on top of whatever the user is doing. When it is dismissed
/// with <c>Escape</c> they should be back where they were, so the window that had the foreground
/// when the widget was activated is captured and restored afterwards.
/// </para>
/// <para>
/// A window that belongs to this process is never captured: if the widget already had the focus
/// there is no other application to go back to, and restoring our own window would leave the
/// user stranded in FluxConvert.
/// </para>
/// </remarks>
public static class ForegroundWindowTracker
{
    /// <summary>
    /// Returns the window that currently has the focus, unless it belongs to this application.
    /// </summary>
    /// <returns>The window handle, or <see cref="IntPtr.Zero"/> when there is nothing to return to.</returns>
    public static IntPtr CapturePrevious()
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        _ = GetWindowThreadProcessId(foreground, out var processId);

        return processId == (uint)Environment.ProcessId ? IntPtr.Zero : foreground;
    }

    /// <summary>
    /// Tries to give the focus back to a window.
    /// </summary>
    /// <param name="window">The window to restore.</param>
    /// <returns><see langword="true"/> when the window has the focus again.</returns>
    public static bool TryRestore(IntPtr window)
    {
        if (window == IntPtr.Zero || !IsWindow(window))
        {
            return false;
        }

        if (SetForegroundWindow(window))
        {
            return true;
        }

        // Windows only lets the foreground thread hand the focus over. Attaching to the thread
        // that currently owns the foreground makes this thread count as the foreground thread for
        // the duration of the call.
        var foreground = GetForegroundWindow();
        if (foreground == window)
        {
            return true;
        }

        var foregroundThread = GetWindowThreadProcessId(foreground, out _);
        var currentThread = GetCurrentThreadId();

        if (foregroundThread == 0 || foregroundThread == currentThread)
        {
            return false;
        }

        if (!AttachThreadInput(currentThread, foregroundThread, true))
        {
            return false;
        }

        try
        {
            return SetForegroundWindow(window);
        }
        finally
        {
            _ = AttachThreadInput(currentThread, foregroundThread, false);
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachThreadInput(
        uint attach,
        uint attachTo,
        [MarshalAs(UnmanagedType.Bool)] bool attachInput);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
}
