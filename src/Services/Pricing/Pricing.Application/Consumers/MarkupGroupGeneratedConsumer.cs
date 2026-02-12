using Contracts.Markup;
using MassTransit;
using MediatR;
using Pricing.Abstractions.Dtos.Markups;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Application.Handlers.Markups.SetGeneratedMarkup;

namespace Pricing.Application.Consumers;

public class MarkupGroupGeneratedConsumer(IMediator mediator, IMarkupSetup markupSetup) : IConsumer<MarkupGroupGeneratedEvent>
{
    public async Task Consume(ConsumeContext<MarkupGroupGeneratedEvent> context)
    {
        var rangesDto = context.Message.MarkupRanges
            .Select(x => new NewMarkupRangeDto
            {
                RangeStart = (double)x.From,
                RangeEnd = (double)x.To,
                MarkupRate = x.Markup
            });
        await mediator.Send(new SetGeneratedMarkupCommand(rangesDto, context.Message.CurrencyId));
        await markupSetup.SetupAsync();
    }
}