using Contracts.Analytics;
using MassTransit;
using MediatR;
using Pricing.Application.Dtos.Markup;
using Pricing.Application.Handlers.Markup.UpsertMarkupGroup;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Application.Consumers;

public class MarkupAnalyzedConsumer(ISender sender, ICachedCurrencyProvider cachedCurrencyProvider)
	: IConsumer<MarkupAnalyzedEvent>
{
	public async Task Consume(ConsumeContext<MarkupAnalyzedEvent> context)
	{
		if (context.Message.Ranges.Count == 0)
			return;

		var messageId = context.MessageId
			?? throw new InvalidOperationException("MarkupAnalyzedEvent requires a MessageId.");
		var baseCurrencyId = await cachedCurrencyProvider.GetBaseCurrencyIdAsync(context.CancellationToken);
		var ranges = context.Message.Ranges.Select(x => new UpsertMarkupRangeDto
		{
			RangeStart = x.FromCost,
			RangeEnd = x.ToCost,
			Markup = x.MeanMarkup
		}).ToArray();

		await sender.Send(new UpsertMarkupGroupCommand(new UpsertMarkupGroupDto
		{
			Name = "Auto Generated Markup",
			CurrencyId = baseCurrencyId,
			Ranges = ranges
		}, messageId),
			context.CancellationToken);
	}
}
