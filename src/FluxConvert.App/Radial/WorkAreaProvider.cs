using System.Runtime.InteropServices;
using System.Windows;

namespace FluxConvert.App.Radial;

/// <summary>
/// Finds the area of the screen a widget may occupy.
/// </summary>
/// <remarks>
/// <para>
/// The work area of the monitor that holds the pointer is used, so the widget appears on the
/// screen the user is looking at. On a mixed-DPI setup the physical work area is converted to
/// device-independent pixels with the effective DPI of that monitor, which keeps the widget the
/// same physical size on every screen.
/// </para>
/// </remarks>
public static class WorkAreaProvider
{
    private const uint MonitorDefaultToNearest = 2;
    private const int MonitorDpiTypeEffective = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect Work;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);

    /// <summary>
    /// Returns the work area of the monitor that currently holds the pointer.
    /// </summary>
    /// <returns>The work area in device-independent pixels.</returns>
    public static Rect GetWorkAreaUnderCursor()
    {
        GetCursorPos(out var cursor);
        return GetWorkArea(new Point(cursor.X, cursor.Y));
    }

    /// <summary>
    /// Returns the work area of the monitor that contains the given device pixel.
    /// </summary>
    /// <param name="devicePoint">A position in physical pixels.</param>
    /// <returns>The work area in device-independent pixels.</returns>
    public static Rect GetWorkArea(Point devicePoint)
    {
        var monitor = MonitorFromPoint(new NativePoint { X = (int)devicePoint.X, Y = (int)devicePoint.Y }, MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
        {
            return new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
        }

        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref info))
        {
            return new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
        }

        var scale = GetScale(monitor);
        var width = (info.Work.Right - info.Work.Left) / scale;
        var height = (info.Work.Bottom - info.Work.Top) / scale;

        return new Rect(
            info.Work.Left / scale,
            info.Work.Top / scale,
            width,
            height);
    }

    private static double GetScale(IntPtr monitor)
    {
        try
        {
            if (GetDpiForMonitor(monitor, MonitorDpiTypeEffective, out var dpiX, out _) == 0 && dpiX > 0)
            {
                return dpiX / 96.0;
            }
        }
        catch (DllNotFoundException)
        {
            // Older Windows versions do not expose the per-monitor DPI. Assume 100 %.
        }
        catch (EntryPointNotFoundException)
        {
            // Older Windows versions do not expose the per-monitor DPI. Assume 100 %.
        }

        return 1.0;
    }
}
