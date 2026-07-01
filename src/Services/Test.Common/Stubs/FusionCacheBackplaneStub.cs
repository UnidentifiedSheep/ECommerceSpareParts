using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Test.Common.Stubs;

public class FusionCacheBackplaneStub : IFusionCacheBackplane
{
    public void Subscribe(BackplaneSubscriptionOptions options) { }

    public ValueTask SubscribeAsync(BackplaneSubscriptionOptions options) { return ValueTask.CompletedTask; }

    public void Unsubscribe() { }

    public ValueTask UnsubscribeAsync() { return ValueTask.CompletedTask; }

    public void Publish(
        BackplaneMessage message,
        FusionCacheEntryOptions options,
        CancellationToken token = default)
    {
    }

    public ValueTask PublishAsync(
        BackplaneMessage message,
        FusionCacheEntryOptions options,
        CancellationToken token = default)
    {
        return ValueTask.CompletedTask;
    }
}