namespace FluxConvert.Core.Layout;

/// <summary>
/// The geometry of the radial interface.
/// </summary>
/// <remarks>
/// The maths lives in the core library on purpose: it is pure, has no user-interface
/// dependency and can therefore be unit tested without a window. The view only converts the
/// results into WPF types.
/// </remarks>
public static class RadialGeometry
{
    /// <summary>
    /// The angle of the first item, measured clockwise from the right.
    /// </summary>
    /// <remarks>
    /// The first item sits at the top of the ring. The keyboard selection uses the same constant,
    /// so the direction an arrow key points at can never drift away from where the items are
    /// actually drawn.
    /// </remarks>
    public const double DefaultStartAngleDegrees = -90.0;

    /// <summary>
    /// A point in device-independent pixels.
    /// </summary>
    /// <param name="X">The horizontal coordinate.</param>
    /// <param name="Y">The vertical coordinate.</param>
    public readonly record struct Point(double X, double Y);

    /// <summary>
    /// A rectangle in device-independent pixels.
    /// </summary>
    /// <param name="X">The left coordinate.</param>
    /// <param name="Y">The top coordinate.</param>
    /// <param name="Width">The width.</param>
    /// <param name="Height">The height.</param>
    public readonly record struct Rect(double X, double Y, double Width, double Height)
    {
        /// <summary>The coordinate just past the right edge.</summary>
        public double Right => X + Width;

        /// <summary>The coordinate just past the bottom edge.</summary>
        public double Bottom => Y + Height;
    }

    /// <summary>The corner of the screen a widget is placed in.</summary>
    public enum Corner
    {
        /// <summary>The bottom right of the work area.</summary>
        BottomRight,

        /// <summary>The bottom left of the work area.</summary>
        BottomLeft,

        /// <summary>The top right of the work area.</summary>
        TopRight,

        /// <summary>The top left of the work area.</summary>
        TopLeft,
    }

    /// <summary>
    /// Returns the centre of an item on the ring.
    /// </summary>
    /// <param name="centreX">The horizontal centre of the ring.</param>
    /// <param name="centreY">The vertical centre of the ring.</param>
    /// <param name="radius">The distance of the item centres from the centre of the ring.</param>
    /// <param name="index">The zero-based index of the item.</param>
    /// <param name="count">The total number of items.</param>
    /// <param name="startAngleDegrees">
    /// The angle of the first item, measured clockwise from the right. The default puts the first
    /// item at the top of the ring.
    /// </param>
    /// <returns>The centre of the item.</returns>
    public static Point GetItemPosition(
        double centreX,
        double centreY,
        double radius,
        int index,
        int count,
        double startAngleDegrees = DefaultStartAngleDegrees)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "There must be at least one item.");
        }

        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "The item index is out of range.");
        }

        var step = 360.0 / count;
        var radians = (startAngleDegrees + (step * index)) * Math.PI / 180.0;

        return new Point(
            centreX + (radius * Math.Cos(radians)),
            centreY + (radius * Math.Sin(radians)));
    }

    /// <summary>
    /// Moves a rectangle so that it lies completely inside a work area.
    /// </summary>
    /// <param name="rect">The rectangle to move.</param>
    /// <param name="workArea">The area the rectangle has to stay inside.</param>
    /// <returns>The moved rectangle.</returns>
    public static Rect ClampToWorkArea(Rect rect, Rect workArea)
    {
        var x = rect.Width >= workArea.Width
            ? workArea.X
            : Math.Clamp(rect.X, workArea.X, workArea.Right - rect.Width);

        var y = rect.Height >= workArea.Height
            ? workArea.Y
            : Math.Clamp(rect.Y, workArea.Y, workArea.Bottom - rect.Height);

        return new Rect(x, y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Places a widget of the given size in a corner of the work area.
    /// </summary>
    /// <param name="size">The size of the widget.</param>
    /// <param name="workArea">The area to place the widget in.</param>
    /// <param name="margin">The gap to keep from the edges of the work area.</param>
    /// <param name="corner">The corner to place the widget in.</param>
    /// <returns>The position of the widget, already clamped to the work area.</returns>
    public static Rect PlaceInCorner(Rect size, Rect workArea, double margin, Corner corner)
    {
        var x = corner is Corner.BottomLeft or Corner.TopLeft
            ? workArea.X + margin
            : workArea.Right - size.Width - margin;

        var y = corner is Corner.TopRight or Corner.TopLeft
            ? workArea.Y + margin
            : workArea.Bottom - size.Height - margin;

        return ClampToWorkArea(new Rect(x, y, size.Width, size.Height), workArea);
    }
}
