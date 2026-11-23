using Abstractions;
using Contracts;
using Contracts.Job;

namespace Gateway.EventStreamBrokers;

public class EventRegistry : EventRegistryBase
{
	public static readonly EventRegistry Instance = new();

	public EventRegistry()
	{
		Add<InAppNotificationCreatedEvent>(
			"onNotificationCreated",
			@event => new { notification = new { id = @event.Id } },
			@event => EventAudience.ForUser(@event.UserId));

		Add<PurchaseCreatedEvent>(
			"onPurchaseCreated",
			@event => new { purchase = new { id = @event.Id } },
			_ => EventAudience.Global);

		Add<SaleCreatedEvent>(
			"onSaleCreated",
			@event => new { sale = new { id = @event.Id } },
			_ => EventAudience.Global);

		Add<SaleUpdatedEvent>(
			"onSaleUpdated",
			@event => new { sale = new { id = @event.Id } },
			_ => EventAudience.Global);

		Add<SaleDeletedEvent>(
			"onSaleDeleted",
			@event => new { sale = new { id = @event.Id } },
			_ => EventAudience.Global);

		Add<PurchaseUpdatedEvent>(
			"onPurchaseUpdated",
			@event => new { purchase = new { id = @event.Id } },
			_ => EventAudience.Global);

		Add<PurchaseDeletedEvent>(
			"onPurchaseDeleted",
			@event => new { purchaseId = @event.Id },
			_ => EventAudience.Global);

		Add<JobStatusUpdatedEvent>(
			"onJobUpdated",
			@event => new { job = new { id = @event.JobId } },
			_ => EventAudience.Global,
			ServicesDefinitions.Main);

		CompleteRegistration();
	}
}
