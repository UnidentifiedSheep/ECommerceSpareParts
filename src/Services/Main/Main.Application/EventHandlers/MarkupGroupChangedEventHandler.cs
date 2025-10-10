using Contracts;
using Core.Interfaces.MessageBroker;
using Main.Core.Interfaces.Pricing;

namespace Main.Application.EventHandlers;

public class MarkupGroupChangedEventHandler(IPriceSetup priceSetup) : IEventHandler<MarkupGroupChangedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupGroupChangedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}