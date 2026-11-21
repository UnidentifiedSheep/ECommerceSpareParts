using Main.Modules.Analytics.Abstractions.Events;
using Main.Modules.Currency.Abstractions.Interfaces;
using MassTransit;
using MediatR;
using Pricing.Application.Commands.Markup.UpsertMarkupGroup;
using Pricing.Application.Dtos.Markup;

namespace Pricing.Application.Consumers;

public class MarkupAnalyzedConsumer(ISender sender, ICurrencyLookup currencyLookup)
	: IConsumer<MarkupAnalyzedEvent>
{
	public async Task Consume(ConsumeContext<MarkupAnalyzedEvent> context)
	{
		if (context.Message.Ranges.Count == 0)
			return;

		var messageId = context.MessageId
			?? throw new InvalidOperationException("MarkupAnalyzedEvent requires a MessageId.");
		var baseCurrencyId = await currencyLookup.GetBaseCurrencyIdAsync(context.CancellationToken);
		var ranges = context.Message
			.Ranges
			.Select(x => new UpsertMarkupRangeDto
			{
				RangeStart = x.FromCost,
				RangeEnd = x.ToCost,
				Markup = x.MeanMarkup
			})
			.ToArray();

		await sender.Send(
			new UpsertMarkupGroupCommand(
				new UpsertMarkupGroupDto
				{
					Name = "Auto Generated Markup",
					CurrencyId = baseCurrencyId,
					Ranges = ranges
				},
				messageId),
			context.CancellationToken);
	}
}
