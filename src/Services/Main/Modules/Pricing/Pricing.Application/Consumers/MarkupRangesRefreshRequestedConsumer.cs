using MassTransit;
using Pricing.Application.Events;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(IMarkupRefresher markupRefresher)
	: IConsumer<MarkupRangesRefreshRequestedEvent>
{
	public Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
		=> markupRefresher.Refresh(context.CancellationToken);
}
