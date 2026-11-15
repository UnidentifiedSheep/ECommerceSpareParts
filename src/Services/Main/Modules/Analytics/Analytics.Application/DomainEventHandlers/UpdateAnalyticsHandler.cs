using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Analytics.Services;
using Main.Modules.Sale.Abstractions;

namespace Main.Application.Analytics.DomainEventHandlers;

public sealed class UpdateAnalyticsHandler(SaleAnalyticsWriter writer)
	: BatchableDomainEventHandler<SaleContentChanged>
{
	public override Task Handle(
		Batch<SaleContentChanged> notification,
		CancellationToken cancellationToken)
		=> writer.WriteAsync(notification.Items.Select(item => item.Snapshot).ToArray(), cancellationToken);
}
