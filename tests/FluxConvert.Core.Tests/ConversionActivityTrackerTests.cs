using FluxConvert.Core.Shell;
using Xunit;

namespace FluxConvert.Core.Tests;

public class ConversionActivityTrackerTests
{
    [Fact]
    public void StartsIdle()
    {
        var tracker = new ConversionActivityTracker();

        Assert.Equal(0, tracker.ActiveCount);
        Assert.False(tracker.IsBusy);
    }

    [Fact]
    public void BecomesBusyWhileAConversionIsRunning()
    {
        var tracker = new ConversionActivityTracker();

        using var scope = tracker.Track();

        Assert.Equal(1, tracker.ActiveCount);
        Assert.True(tracker.IsBusy);
    }

    [Fact]
    public void BecomesIdleAgainWhenTheScopeIsDisposed()
    {
        var tracker = new ConversionActivityTracker();
        var scope = tracker.Track();

        scope.Dispose();

        Assert.Equal(0, tracker.ActiveCount);
        Assert.False(tracker.IsBusy);
    }

    [Fact]
    public void CountsEveryRunningConversion()
    {
        var tracker = new ConversionActivityTracker();

        using var first = tracker.Track();
        using var second = tracker.Track();

        Assert.Equal(2, tracker.ActiveCount);
    }

    [Fact]
    public void DisposingAScopeTwiceDoesNotDoubleCount()
    {
        var tracker = new ConversionActivityTracker();
        var scope = tracker.Track();

        scope.Dispose();
        scope.Dispose();

        Assert.Equal(0, tracker.ActiveCount);
    }
}
