using FluxConvert.Core.Shell;
using Xunit;

namespace FluxConvert.Core.Tests;

public class SingleInstanceCoordinatorTests
{
    private static string NewApplicationName() => "FluxConvertTest-" + Guid.NewGuid().ToString("N");

    [Fact]
    public void TheFirstInstanceBecomesThePrimaryOne()
    {
        var name = NewApplicationName();

        using var first = new SingleInstanceCoordinator(name);
        using var second = new SingleInstanceCoordinator(name);

        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);
    }

    [Fact]
    public void ThePrimaryIsReleasedWhenItIsDisposed()
    {
        var name = NewApplicationName();

        using (var first = new SingleInstanceCoordinator(name))
        {
            Assert.True(first.IsPrimary);
        }

        using var second = new SingleInstanceCoordinator(name);

        Assert.True(second.IsPrimary);
    }

    [Fact]
    public async Task SignallingActivatesThePrimaryInstance()
    {
        var name = NewApplicationName();
        using var primary = new SingleInstanceCoordinator(name);
        using var secondary = new SingleInstanceCoordinator(name);

        var activated = new TaskCompletionSource();
        using var listener = primary.StartActivationListener(() => activated.TrySetResult());

        var delivered = SingleInstanceCoordinator.SignalPrimary(name);

        Assert.True(delivered);

        var finished = await Task.WhenAny(activated.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(activated.Task, finished);
    }

    [Fact]
    public void SignallingWithoutAPrimaryInstanceIsHarmless()
    {
        var delivered = SingleInstanceCoordinator.SignalPrimary(NewApplicationName());

        Assert.False(delivered);
    }

    [Fact]
    public void StoppingTheListenerPreventsFurtherActivations()
    {
        var name = NewApplicationName();
        using var primary = new SingleInstanceCoordinator(name);

        var activations = 0;
        var listener = primary.StartActivationListener(() => Interlocked.Increment(ref activations));
        listener.Dispose();

        SingleInstanceCoordinator.SignalPrimary(name);
        Thread.Sleep(300);

        Assert.Equal(0, Volatile.Read(ref activations));
    }

    [Fact]
    public void TheSystemObjectNamesAreSessionScoped()
    {
        Assert.StartsWith(@"Local\", SingleInstanceCoordinator.BuildMutexName("FluxConvert"), StringComparison.Ordinal);
        Assert.StartsWith(@"Local\", SingleInstanceCoordinator.BuildEventName("FluxConvert"), StringComparison.Ordinal);
    }
}
