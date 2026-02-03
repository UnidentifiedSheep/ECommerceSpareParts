using Contracts.Markup;
using Core.Interfaces.MessageBroker;
using Main.Abstractions.Interfaces.Pricing;

namespace Main.Application.EventHandlers;

public class MarkupRangesChangedEventHandler(IMarkupSetup markupSetup) : IEventHandler<MarkupRangesUpdatedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupRangesUpdatedEvent> context)
    {
        await markupSetup.SetupAsync();
    }
}