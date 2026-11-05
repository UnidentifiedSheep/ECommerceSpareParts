using Contracts;

namespace Gateway.EventStreamBrokers;

public class EventRegistry : EventRegistryBase
{
	public static readonly EventRegistry Instance = new();

	public EventRegistry()
	{
		Add<InAppNotificationCreatedEvent>(
			topic: "onNotificationCreated",
			mapper: @event => new { notification = new { id = @event.Id } },
			resolveAudience: @event => EventAudience.ForUser(@event.UserId));

		Add<PurchaseCreatedEvent>(
			topic: "onPurchaseCreated",
			mapper: @event => new { purchase = new { id = @event.Id } },
			resolveAudience: _ => EventAudience.Global);

		Add<SaleCreatedEvent>(
			topic: "onSaleCreated",
			mapper: @event => new { sale = new { id = @event.Id } },
			resolveAudience: _ => EventAudience.Global);

		Add<SaleUpdatedEvent>(
			topic: "onSaleUpdated",
			mapper: @event => new { sale = new { id = @event.Id } },
			resolveAudience: _ => EventAudience.Global);

		Add<SaleDeletedEvent>(
			topic: "onSaleDeleted",
			mapper: @event => new { sale = new { id = @event.Id } },
			resolveAudience: _ => EventAudience.Global);

		Add<PurchaseUpdatedEvent>(
			topic: "onPurchaseUpdated",
			mapper: @event => new { purchase = new { id = @event.Id } },
			resolveAudience: _ => EventAudience.Global);

		Add<PurchaseDeletedEvent>(
			topic: "onPurchaseDeleted",
			mapper: @event => new { purchaseId = @event.Id },
			resolveAudience: _ => EventAudience.Global);

		CompleteRegistration();
	}
}
