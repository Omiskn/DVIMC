using FluxConvert.Core.Layout;
using Xunit;

namespace FluxConvert.Core.Tests;

public class RadialGeometryTests
{
    [Fact]
    public void TheFirstItemSitsAtTheTopOfTheRing()
    {
        var position = RadialGeometry.GetItemPosition(160, 160, 123, 0, 4);

        Assert.Equal(160, position.X, 6);
        Assert.Equal(37, position.Y, 6);
    }

    [Fact]
    public void FourItemsAreSpreadEvenlyAroundTheRing()
    {
        var positions = Enumerable.Range(0, 4)
            .Select(index => RadialGeometry.GetItemPosition(0, 0, 100, index, 4))
            .ToArray();

        // Top, right, bottom, left.
        Assert.Equal(0, positions[0].X, 6);
        Assert.Equal(-100, positions[0].Y, 6);
        Assert.Equal(100, positions[1].X, 6);
        Assert.Equal(0, positions[1].Y, 6);
        Assert.Equal(0, positions[2].X, 6);
        Assert.Equal(100, positions[2].Y, 6);
        Assert.Equal(-100, positions[3].X, 6);
        Assert.Equal(0, positions[3].Y, 6);
    }

    [Fact]
    public void EveryItemIsTheSameDistanceFromTheCentre()
    {
        for (var index = 0; index < 7; index++)
        {
            var position = RadialGeometry.GetItemPosition(12, 34, 123, index, 7);

            var distance = Math.Sqrt(Math.Pow(position.X - 12, 2) + Math.Pow(position.Y - 34, 2));

            Assert.Equal(123, distance, 6);
        }
    }

    [Fact]
    public void ASingleItemSitsAtTheTop()
    {
        var position = RadialGeometry.GetItemPosition(50, 50, 80, 0, 1);

        Assert.Equal(50, position.X, 6);
        Assert.Equal(-30, position.Y, 6);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 1)]
    [InlineData(3, 3)]
    public void AnInvalidIndexIsRejected(int index, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RadialGeometry.GetItemPosition(0, 0, 100, index, count));
    }

    [Fact]
    public void AWidgetIsMovedBackInsideTheWorkArea()
    {
        var workArea = new RadialGeometry.Rect(0, 0, 1920, 1040);
        var widget = new RadialGeometry.Rect(1900, 1030, 348, 416);

        var clamped = RadialGeometry.ClampToWorkArea(widget, workArea);

        Assert.Equal(1920 - 348, clamped.X, 6);
        Assert.Equal(1040 - 416, clamped.Y, 6);
    }

    [Fact]
    public void AWidgetThatAlreadyFitsIsNotMoved()
    {
        var workArea = new RadialGeometry.Rect(0, 0, 1920, 1040);
        var widget = new RadialGeometry.Rect(100, 100, 348, 416);

        var clamped = RadialGeometry.ClampToWorkArea(widget, workArea);

        Assert.Equal(100, clamped.X, 6);
        Assert.Equal(100, clamped.Y, 6);
    }

    [Fact]
    public void AWidgetLargerThanTheWorkAreaIsPinnedToTheTopLeft()
    {
        var workArea = new RadialGeometry.Rect(50, 60, 100, 100);
        var widget = new RadialGeometry.Rect(0, 0, 200, 300);

        var clamped = RadialGeometry.ClampToWorkArea(widget, workArea);

        Assert.Equal(50, clamped.X, 6);
        Assert.Equal(60, clamped.Y, 6);
    }

    [Fact]
    public void AWidgetIsPlacedInTheBottomRightCorner()
    {
        var workArea = new RadialGeometry.Rect(0, 0, 1920, 1040);
        var size = new RadialGeometry.Rect(0, 0, 348, 416);

        var placed = RadialGeometry.PlaceInCorner(size, workArea, 16, RadialGeometry.Corner.BottomRight);

        Assert.Equal(1920 - 348 - 16, placed.X, 6);
        Assert.Equal(1040 - 416 - 16, placed.Y, 6);
    }

    [Fact]
    public void AWidgetIsPlacedInTheTopLeftCorner()
    {
        var workArea = new RadialGeometry.Rect(0, 0, 1920, 1040);
        var size = new RadialGeometry.Rect(0, 0, 348, 416);

        var placed = RadialGeometry.PlaceInCorner(size, workArea, 16, RadialGeometry.Corner.TopLeft);

        Assert.Equal(16, placed.X, 6);
        Assert.Equal(16, placed.Y, 6);
    }

    [Fact]
    public void AWidgetPlacedInACornerIsStillFullyVisible()
    {
        var workArea = new RadialGeometry.Rect(0, 0, 800, 600);
        var size = new RadialGeometry.Rect(0, 0, 348, 416);

        var placed = RadialGeometry.PlaceInCorner(size, workArea, 16, RadialGeometry.Corner.BottomRight);

        Assert.True(placed.X >= workArea.X);
        Assert.True(placed.Y >= workArea.Y);
        Assert.True(placed.Right <= workArea.Right);
        Assert.True(placed.Bottom <= workArea.Bottom);
    }
}
