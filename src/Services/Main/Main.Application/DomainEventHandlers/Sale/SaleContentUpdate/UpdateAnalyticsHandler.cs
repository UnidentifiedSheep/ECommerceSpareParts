using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Analytics.Interfaces;
using Main.Entities.DomainEvents.Sale;

namespace Main.Application.DomainEventHandlers.Sale.SaleContentUpdate;

public sealed class UpdateAnalyticsHandler(ISaleAnalyticsSynchronizer synchronizer)
	: BatchableDomainEventHandler<SaleContentUpdateDomainEvent>
{
	public override Task Handle(Batch<SaleContentUpdateDomainEvent> notification, CancellationToken cancellationToken)
	{
		var contents = notification.Items
			.Where(item => item.FinancialInfoChanged)
			.Select(item => item.Content)
			.DistinctBy(content => content.Id)
			.ToArray();
		return synchronizer.SynchronizeAsync(contents, cancellationToken);
	}
}
