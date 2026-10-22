using System.Windows;
using System.Windows.Media;

namespace FluxConvert.App.Radial;

/// <summary>
/// Draws the glass ring of the radial interface.
/// </summary>
/// <remarks>
/// The ring is a single annulus: the outer circle with the inner circle removed. Drawing it as
/// one shape keeps the inner edge crisp and avoids the halo a semi-transparent ellipse would
/// leave where the two shapes overlap.
/// </remarks>
public class RingSurface : FrameworkElement
{
    /// <summary>Initialises a new instance of the <see cref="RingSurface"/> class.</summary>
    public RingSurface()
    {
        SnapsToDevicePixels = true;
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);

        var outerRadius = ResourceDouble("Size.RingOuterRadius", 150);
        var innerRadius = ResourceDouble("Size.RingInnerRadius", 96);
        var centre = new Point(ActualWidth / 2.0, ActualHeight / 2.0);

        var outer = new EllipseGeometry(new Rect(
            centre.X - outerRadius,
            centre.Y - outerRadius,
            outerRadius * 2,
            outerRadius * 2));

        var inner = new EllipseGeometry(new Rect(
            centre.X - innerRadius,
            centre.Y - innerRadius,
            innerRadius * 2,
            innerRadius * 2));

        var ring = new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);

        var fill = FindBrush("Gradient.Ring") ?? FindBrush("Surface.Ring");
        if (fill is not null)
        {
            drawingContext.DrawGeometry(fill, null, ring);
        }

        var border = FindBrush("Border.Subtle");
        if (border is not null)
        {
            var pen = new Pen(border, 1) { LineJoin = PenLineJoin.Round };
            drawingContext.DrawGeometry(null, pen, ring);
        }

        // A hairline on the inner edge gives the hole a defined rim.
        var innerBorder = FindBrush("Border.Subtle");
        if (innerBorder is not null)
        {
            var pen = new Pen(innerBorder, 1);
            drawingContext.DrawEllipse(null, pen, centre, innerRadius, innerRadius);
        }
    }

    private double ResourceDouble(string key, double fallback) =>
        TryFindResource(key) is double value ? value : fallback;

    private Brush? FindBrush(string key) => TryFindResource(key) as Brush;
}
