using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common.Backplane;

public interface IBackplaneDispatcher
{
    Func<BackplaneMessage, ValueTask>? Handler { get; }
    void Subscribe(BackplaneSubscriptionOptions options);
    void Unsubscribe();
}

public class BackplaneDispatcher : IBackplaneDispatcher
{
    public Func<BackplaneMessage, ValueTask>? Handler { get; set; }

    public void Subscribe(BackplaneSubscriptionOptions options)
    {
        Handler = options.IncomingMessageHandlerAsync;
    }

    public void Unsubscribe()
    {
        Handler = null;
    }
}