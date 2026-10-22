using FluxConvert.Core.Layout;
using Xunit;

namespace FluxConvert.Core.Tests;

public class RingSelectionTests
{
    private static readonly bool[] FourItems = [true, true, true, true];

    [Theory]
    [InlineData(RingDirection.Up, 0)]
    [InlineData(RingDirection.Right, 1)]
    [InlineData(RingDirection.Down, 2)]
    [InlineData(RingDirection.Left, 3)]
    public void EachArrowKeyPointsAtTheItemInThatDirection(RingDirection direction, int expected)
    {
        var index = RingSelection.Move(direction, -1, FourItems);

        Assert.Equal<int?>(expected, index);
    }

    [Theory]
    [InlineData(RingDirection.Up, 0)]
    [InlineData(RingDirection.Right, 2)]
    [InlineData(RingDirection.Down, 4)]
    [InlineData(RingDirection.Left, 6)]
    public void TheDirectionIsIndependentOfTheNumberOfItems(RingDirection direction, int expected)
    {
        var eightItems = new bool[8];
        Array.Fill(eightItems, true);

        var index = RingSelection.Move(direction, -1, eightItems);

        Assert.Equal<int?>(expected, index);
    }

    [Fact]
    public void ThreeItemsSnapToTheNearestDirection()
    {
        var threeItems = new[] { true, true, true };

        Assert.Equal<int?>(0, RingSelection.Move(RingDirection.Up, -1, threeItems));
        Assert.Equal<int?>(1, RingSelection.Move(RingDirection.Right, -1, threeItems));

        // Down and left both point closest at the lower left item; down is the tie-break.
        Assert.Equal<int?>(2, RingSelection.Move(RingDirection.Down, -1, threeItems));
        Assert.Equal<int?>(2, RingSelection.Move(RingDirection.Left, -1, threeItems));
    }

    [Fact]
    public void ADisabledItemIsNeverReturned()
    {
        var items = new[] { true, false, true, true };

        foreach (var direction in Enum.GetValues<RingDirection>())
        {
            var index = RingSelection.Move(direction, -1, items);

            Assert.True(index.HasValue, $"{direction} did not find an item.");
            Assert.True(items[index.GetValueOrDefault()], $"{direction} returned a disabled item.");
        }
    }

    [Fact]
    public void WhenTheItemInThatDirectionIsDisabledTheNeighbourTakesOver()
    {
        var items = new[] { true, false, true, true };

        // The top and the bottom item are both 90 degrees from the right; clockwise wins.
        Assert.Equal<int?>(2, RingSelection.Move(RingDirection.Right, -1, items));
    }

    [Fact]
    public void ATieMovesAwayFromTheCurrentItem()
    {
        var items = new[] { true, false, true, true };

        Assert.Equal<int?>(0, RingSelection.Move(RingDirection.Right, 2, items));
        Assert.Equal<int?>(2, RingSelection.Move(RingDirection.Right, 0, items));
    }

    [Fact]
    public void PressingTheDirectionOfTheFocusedItemKeepsItFocused()
    {
        Assert.Equal<int?>(0, RingSelection.Move(RingDirection.Up, 0, FourItems));
        Assert.Equal<int?>(1, RingSelection.Move(RingDirection.Right, 1, FourItems));
    }

    [Fact]
    public void ASingleEnabledItemIsAlwaysTheAnswer()
    {
        var items = new[] { false, false, true, false };

        foreach (var direction in Enum.GetValues<RingDirection>())
        {
            Assert.Equal<int?>(2, RingSelection.Move(direction, -1, items));
            Assert.Equal<int?>(2, RingSelection.Move(direction, 2, items));
        }
    }

    [Fact]
    public void NoEnabledItemsMeansNoSelection()
    {
        var items = new[] { false, false, false, false };

        foreach (var direction in Enum.GetValues<RingDirection>())
        {
            Assert.Null(RingSelection.Move(direction, -1, items));
        }
    }

    [Fact]
    public void AnEmptyRingMeansNoSelection()
    {
        Assert.Null(RingSelection.Move(RingDirection.Up, -1, Array.Empty<bool>()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    public void AnOutOfRangeFocusedIndexStillProducesAValidItem(int currentIndex)
    {
        var index = RingSelection.Move(RingDirection.Right, currentIndex, FourItems);

        Assert.Equal<int?>(1, index);
    }

    [Fact]
    public void AnUnknownDirectionIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RingSelection.Move((RingDirection)99, -1, FourItems));
    }

    [Fact]
    public void ANullEnabledListIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => RingSelection.Move(RingDirection.Up, -1, null!));
        Assert.Throws<ArgumentNullException>(() => RingSelection.First(null!));
        Assert.Throws<ArgumentNullException>(() => RingSelection.Last(null!));
    }

    [Fact]
    public void HomeFindsTheFirstEnabledItem()
    {
        Assert.Equal<int?>(1, RingSelection.First([false, true, true, false]));
    }

    [Fact]
    public void EndFindsTheLastEnabledItem()
    {
        Assert.Equal<int?>(2, RingSelection.Last([false, true, true, false]));
    }

    [Fact]
    public void HomeAndEndFindNothingWhenEverythingIsDisabled()
    {
        Assert.Null(RingSelection.First([false, false, false]));
        Assert.Null(RingSelection.Last([false, false, false]));
        Assert.Null(RingSelection.First(Array.Empty<bool>()));
        Assert.Null(RingSelection.Last(Array.Empty<bool>()));
    }

    [Fact]
    public void EveryPatternOfFiveItemsOnlyEverSelectsAnEnabledItem()
    {
        for (var mask = 0; mask < 32; mask++)
        {
            var items = new bool[5];
            for (var index = 0; index < items.Length; index++)
            {
                items[index] = (mask & (1 << index)) != 0;
            }

            var anyEnabled = items.Any(enabled => enabled);

            foreach (var direction in Enum.GetValues<RingDirection>())
            {
                var selected = RingSelection.Move(direction, -1, items);

                if (!anyEnabled)
                {
                    Assert.Null(selected);
                    continue;
                }

                Assert.True(selected.HasValue, $"{direction} did not find an item in pattern {mask}.");
                Assert.True(items[selected.GetValueOrDefault()], $"{direction} returned a disabled item.");
            }
        }
    }
}
