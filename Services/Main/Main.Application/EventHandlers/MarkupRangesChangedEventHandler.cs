using Core.Contracts;
using Core.Interfaces;
using Core.Interfaces.MessageBroker;

namespace Main.Application.EventHandlers;

public class MarkupRangesChangedEventHandler(IPriceSetup priceSetup) : IEventHandler<MarkupRangesUpdatedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupRangesUpdatedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}