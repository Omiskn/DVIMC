using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using DrawingColor = System.Drawing.Color;
using ImageSource = System.Windows.Media.ImageSource;

namespace FluxConvert.App.Shell;

/// <summary>
/// Draws the application icon at run time.
/// </summary>
/// <remarks>
/// <para>
/// The icon is generated in code instead of being shipped as a binary asset. That keeps the
/// repository free of image files, lets the icon follow the colour tokens, and produces a sharp
/// result at every size the notification area asks for.
/// </para>
/// <para>
/// The mark echoes the radial interface: a gradient ring, a solid core and a single accent dot
/// sitting on the ring.
/// </para>
/// </remarks>
public static class AppIconFactory
{
    private static readonly DrawingColor RingStart = ColorTranslator.FromHtml("#4C8DFF");
    private static readonly DrawingColor RingEnd = ColorTranslator.FromHtml("#8B6CF0");
    private static readonly DrawingColor Core = ColorTranslator.FromHtml("#141A2B");
    private static readonly DrawingColor Accent = ColorTranslator.FromHtml("#2DD4BF");

    /// <summary>
    /// Draws the icon into a new bitmap.
    /// </summary>
    /// <param name="size">Width and height in pixels.</param>
    /// <returns>The bitmap. The caller owns it.</returns>
    public static Bitmap CreateBitmap(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "The icon size must be positive.");
        }

        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(DrawingColor.Transparent);

        var padding = size * 0.06f;
        var bounds = new RectangleF(padding, padding, size - (2 * padding), size - (2 * padding));
        var ringThickness = Math.Max(1.5f, size * 0.13f);

        using (var gradient = new LinearGradientBrush(bounds, RingStart, RingEnd, 55f))
        using (var pen = new Pen(gradient, ringThickness))
        {
            graphics.DrawEllipse(pen, bounds);
        }

        var core = RectangleF.Inflate(bounds, -ringThickness, -ringThickness);
        using (var brush = new SolidBrush(Core))
        {
            graphics.FillEllipse(brush, core);
        }

        var dotRadius = ringThickness * 0.6f;
        var dotCentreX = bounds.X + (bounds.Width / 2f);
        var dotCentreY = bounds.Y + (ringThickness / 2f);
        using (var brush = new SolidBrush(Accent))
        {
            graphics.FillEllipse(
                brush,
                dotCentreX - dotRadius,
                dotCentreY - dotRadius,
                dotRadius * 2,
                dotRadius * 2);
        }

        return bitmap;
    }

    /// <summary>
    /// Renders the icon as a WPF image source, for example for a window title bar.
    /// </summary>
    /// <param name="size">Width and height in pixels.</param>
    /// <returns>A frozen image source.</returns>
    public static ImageSource CreateImageSource(int size)
    {
        using var bitmap = CreateBitmap(size);
        var handle = bitmap.GetHbitmap();
        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            NativeMethods.DeleteObject(handle);
        }
    }

    /// <summary>
    /// Creates a Windows icon handle for the notification area.
    /// </summary>
    /// <param name="size">Width and height in pixels.</param>
    /// <returns>An icon lease that must be disposed.</returns>
    public static IconLease CreateIcon(int size)
    {
        using var bitmap = CreateBitmap(size);
        var handle = bitmap.GetHicon();
        try
        {
            return new IconLease(handle);
        }
        catch
        {
            NativeMethods.DestroyIcon(handle);
            throw;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr handle);
    }
}

/// <summary>
/// Owns a Windows icon handle and the managed wrapper around it.
/// </summary>
public sealed class IconLease : IDisposable
{
    private readonly IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="IconLease"/> class.
    /// </summary>
    /// <param name="handle">The native icon handle.</param>
    public IconLease(IntPtr handle)
    {
        _handle = handle;
        Icon = System.Drawing.Icon.FromHandle(handle);
    }

    /// <summary>The managed icon.</summary>
    public System.Drawing.Icon Icon { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Icon.Dispose();
        AppIconFactory.NativeMethods.DestroyIcon(_handle);
    }
}
