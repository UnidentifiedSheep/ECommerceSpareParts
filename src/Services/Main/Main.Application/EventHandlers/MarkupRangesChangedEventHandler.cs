using Contracts;
using Core.Interfaces.MessageBroker;
using Main.Core.Interfaces.Pricing;

namespace Main.Application.EventHandlers;

public class MarkupRangesChangedEventHandler(IPriceSetup priceSetup) : IEventHandler<MarkupRangesUpdatedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupRangesUpdatedEvent> context)
    {
        await priceSetup.SetupAsync();
    }
}