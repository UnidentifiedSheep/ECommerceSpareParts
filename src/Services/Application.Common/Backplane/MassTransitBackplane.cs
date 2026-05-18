using MassTransit;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common.Backplane;

public class MassTransitBackplane(IBus bus, IBackplaneDispatcher dispatcher) : IFusionCacheBackplane
{
    public void Subscribe(BackplaneSubscriptionOptions options)
    {
        dispatcher.Subscribe(options);
    }

    public ValueTask SubscribeAsync(BackplaneSubscriptionOptions options)
    {
        dispatcher.Subscribe(options);
        return ValueTask.CompletedTask;
    }

    public void Unsubscribe()
    {
        dispatcher.Unsubscribe();
    }

    public ValueTask UnsubscribeAsync()
    {
        dispatcher.Unsubscribe();
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishAsync(
        BackplaneMessage message,
        FusionCacheEntryOptions options,
        CancellationToken token = new())
    {
        return new ValueTask(bus.Publish(message, token));
    }

    public void Publish(
        BackplaneMessage message,
        FusionCacheEntryOptions options,
        CancellationToken token = new())
    {
        _ = Task.Run(async () => await PublishAsync(message, options, token), token);
    }
}
