namespace FluxConvert.Core.Layout;

/// <summary>
/// Chooses which item of the ring a keyboard action should focus.
/// </summary>
/// <remarks>
/// <para>
/// The arrow keys work spatially rather than sequentially: each direction points at the item that
/// sits closest to that edge of the ring. That is what a radial menu leads a user to expect, and
/// it means an item can be reached with a single key press. Sequential movement stays available
/// with <c>Tab</c>, and <see cref="First"/> and <see cref="Last"/> back <c>Home</c> and <c>End</c>.
/// </para>
/// <para>
/// Disabled items are never returned, so the keyboard cannot land on something that cannot be
/// used. When two enabled items are equally close to a direction, the item that is not currently
/// focused wins — pressing a direction should move the focus whenever there is somewhere to move
/// to — and any remaining tie goes to the item clockwise of the requested direction.
/// </para>
/// <para>
/// The maths is deliberately pure and lives in the core library, so the keyboard model can be
/// unit tested without a window. It shares <see cref="RadialGeometry.DefaultStartAngleDegrees"/>
/// with the layout, so the selection can never drift away from where the items are drawn.
/// </para>
/// </remarks>
public static class RingSelection
{
    private const double Tolerance = 1e-9;

    /// <summary>
    /// Returns the index of the first enabled item.
    /// </summary>
    /// <param name="enabled">Whether each item can be used, in ring order.</param>
    /// <returns>The index of the first enabled item, or <see langword="null"/> when there is none.</returns>
    public static int? First(IReadOnlyList<bool> enabled)
    {
        ArgumentNullException.ThrowIfNull(enabled);

        for (var index = 0; index < enabled.Count; index++)
        {
            if (enabled[index])
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the index of the last enabled item.
    /// </summary>
    /// <param name="enabled">Whether each item can be used, in ring order.</param>
    /// <returns>The index of the last enabled item, or <see langword="null"/> when there is none.</returns>
    public static int? Last(IReadOnlyList<bool> enabled)
    {
        ArgumentNullException.ThrowIfNull(enabled);

        for (var index = enabled.Count - 1; index >= 0; index--)
        {
            if (enabled[index])
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the enabled item that lies closest to the given direction.
    /// </summary>
    /// <param name="direction">The direction the user asked for.</param>
    /// <param name="currentIndex">
    /// The index of the focused item, or a value outside the ring when nothing is focused yet.
    /// </param>
    /// <param name="enabled">Whether each item can be used, in ring order.</param>
    /// <returns>The index to focus, or <see langword="null"/> when no item can be used.</returns>
    public static int? Move(RingDirection direction, int currentIndex, IReadOnlyList<bool> enabled)
    {
        ArgumentNullException.ThrowIfNull(enabled);

        var count = enabled.Count;
        if (count == 0)
        {
            return null;
        }

        var target = direction switch
        {
            RingDirection.Up => 270.0,
            RingDirection.Right => 0.0,
            RingDirection.Down => 90.0,
            RingDirection.Left => 180.0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "The direction is not a ring direction."),
        };

        var bestIndex = -1;
        var bestDistance = double.MaxValue;
        var bestSignedDistance = 0.0;
        var bestIsCurrent = true;

        for (var index = 0; index < count; index++)
        {
            if (!enabled[index])
            {
                continue;
            }

            var signedDistance = AngularDistance(target, AngleOf(index, count));
            var distance = Math.Abs(signedDistance);
            var isCurrent = index == currentIndex;

            if (bestIndex < 0 || IsBetter(distance, signedDistance, isCurrent))
            {
                bestIndex = index;
                bestDistance = distance;
                bestSignedDistance = signedDistance;
                bestIsCurrent = isCurrent;
            }
        }

        return bestIndex < 0 ? null : bestIndex;

        bool IsBetter(double distance, double signedDistance, bool isCurrent)
        {
            if (Math.Abs(distance - bestDistance) > Tolerance)
            {
                return distance < bestDistance;
            }

            if (isCurrent != bestIsCurrent)
            {
                return !isCurrent;
            }

            if (Math.Abs(signedDistance - bestSignedDistance) > Tolerance)
            {
                return signedDistance > bestSignedDistance;
            }

            return false;
        }
    }

    /// <summary>
    /// Returns the angle of an item, measured clockwise from the right and normalised to
    /// <c>[0, 360)</c>.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    /// <param name="count">The number of items on the ring.</param>
    /// <returns>The angle in degrees.</returns>
    private static double AngleOf(int index, int count)
    {
        var angle = RadialGeometry.DefaultStartAngleDegrees + (360.0 * index / count);
        angle %= 360.0;

        return angle < 0 ? angle + 360.0 : angle;
    }

    /// <summary>
    /// Returns the shortest signed rotation from one angle to another, in <c>(-180, 180]</c>.
    /// A positive value is clockwise.
    /// </summary>
    /// <param name="from">The angle to start at, in degrees.</param>
    /// <param name="to">The angle to end at, in degrees.</param>
    /// <returns>The signed rotation in degrees.</returns>
    private static double AngularDistance(double from, double to)
    {
        var delta = (to - from) % 360.0;

        if (delta > 180.0)
        {
            delta -= 360.0;
        }
        else if (delta < -180.0)
        {
            delta += 360.0;
        }

        return delta;
    }
}
