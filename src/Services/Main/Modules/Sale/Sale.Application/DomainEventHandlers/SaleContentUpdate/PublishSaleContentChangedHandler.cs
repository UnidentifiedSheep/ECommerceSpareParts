using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Entities.Sale.DomainEvents;
using Main.Modules.Sale.Abstractions;
using Main.Modules.Sale.Application.Services;

namespace Main.Modules.Sale.Application.DomainEventHandlers.SaleContentUpdate;

public sealed class PublishSaleContentChangedHandler(IDomainEventScope eventScope)
	: BatchableDomainEventHandler<SaleContentUpdateDomainEvent>
{
	public override Task Handle(
		Batch<SaleContentUpdateDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var contents = notification.Items
			.Where(item => item.FinancialInfoChanged)
			.Select(item => item.Content)
			.DistinctBy(content => content.Id)
			.Where(content => content.Sale is { } sale && sale.Contents.Contains(content));

		foreach (var content in contents)
			eventScope.Add(new SaleContentChanged(SaleContentSnapshots.From(content)));

		return Task.CompletedTask;
	}
}
