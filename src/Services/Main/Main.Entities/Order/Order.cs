using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Enums.Orders;

namespace Main.Entities.Order;

public class Order : AuditableEntity<Order, Guid>, ILinqEntity<Order, Guid>
{
	public Guid Id { get; private set; }
	public Guid OrganizationId { get; private set; }
	public Guid? UserId { get; private set; }
	public int CurrencyId { get; private set; }
	public OrderSource Source { get; private set; }
	public OrderStatus Status { get; private set; }
	public OrderFulfillmentStatus FulfillmentStatus { get; private set; }

	public Guid? ConfirmedByUserId { get; private set; }
	public DateTime? ConfirmedAt { get; private set; }

	private readonly List<OrderItem> _items = [];
	public IReadOnlyList<OrderItem> Items => _items;

	private Order() {}

	private Order(
		Guid organizationId,
		Guid? userId,
		int currencyId,
		OrderSource source)
	{
		Id = Guid.NewGuid();
		OrganizationId = organizationId;
		UserId = userId;
		CurrencyId = currencyId;
		Source = source;
		Status = OrderStatus.Pending;
		FulfillmentStatus = OrderFulfillmentStatus.NotAvailable;
	}

	public static Order CreateManual(Guid organizationId, Guid? userId, int currencyId)
		=> new(organizationId, userId, currencyId, OrderSource.Manual);

	public static Order CreateOnline(Guid organizationId, Guid userId, int currencyId)
		=> new(organizationId, userId, currencyId, OrderSource.Online);

	public void AddItem(OrderItem item)
	{
		if (Status != OrderStatus.Pending)
			throw new InvalidOperationException($"Cannot add items to an order in '{Status}' status.");

		if (item.OrderId != Id)
			throw new InvalidOperationException("Order id miss match");

		_items.Add(item);
	}

	public void Confirm(Guid? confirmedBy)
	{
		if (Status != OrderStatus.Pending)
			throw new InvalidOperationException($"Cannot confirm an order in '{Status}' status.");

		Status = OrderStatus.Confirmed;
		ConfirmedAt = DateTime.UtcNow;
		ConfirmedByUserId = confirmedBy;
	}

	public void Complete()
	{
		if (Status != OrderStatus.Confirmed)
			throw new InvalidOperationException($"Cannot complete an order in '{Status}' status.");

		Status = OrderStatus.Completed;
	}

	public void Cancel()
	{
		if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
			throw new InvalidOperationException($"Cannot cancel an order in '{Status}' status.");

		Status = OrderStatus.Cancelled;
	}

	public void SetFulfillmentStatus(OrderFulfillmentStatus status)
	{
		if (Status is OrderStatus.Completed or OrderStatus.Cancelled)
			throw new InvalidOperationException(
				$"Cannot update fulfillment status of an order in '{Status}' status.");

		FulfillmentStatus = status;
	}

	public static Expression<Func<Order, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Order, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public override Guid GetId() => Id;
}
