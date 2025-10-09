using Core.Contracts;
using Core.Interfaces;
using Core.Interfaces.MessageBroker;

namespace Main.Application.EventHandlers;

public class MarkupGroupChangedEventHandler(IPriceSetup priceSetup) : IEventHandler<MarkupGroupChangedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupGroupChangedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}