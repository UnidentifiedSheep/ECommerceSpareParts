using Contracts.Markup;
using Core.Interfaces.MessageBroker;
using Main.Abstractions.Interfaces.Pricing;

namespace Main.Application.EventHandlers;

public class MarkupGroupChangedEventHandler(IMarkupSetup markupSetup) : IEventHandler<MarkupGroupChangedEvent>
{
    public async Task HandleAsync(IEventContext<MarkupGroupChangedEvent> context)
    {
        await markupSetup.SetupAsync();
    }
}