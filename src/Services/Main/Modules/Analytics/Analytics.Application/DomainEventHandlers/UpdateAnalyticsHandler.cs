using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Modules.Analytics.Application.Services;
using Main.Modules.Sale.Abstractions;

namespace Main.Modules.Analytics.Application.DomainEventHandlers;

public sealed class UpdateAnalyticsHandler(SaleAnalyticsWriter writer)
	: BatchableDomainEventHandler<SaleContentChanged>
{
	public override Task Handle(
		Batch<SaleContentChanged> notification,
		CancellationToken cancellationToken)
		=> writer.WriteAsync(notification.Items.Select(item => item.Snapshot).ToArray(), cancellationToken);
}
