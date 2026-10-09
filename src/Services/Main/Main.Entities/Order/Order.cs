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
	public DateTimeOffset? ConfirmedAt { get; private set; }

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

	public static Order CreateManual(
		Guid organizationId,
		Guid? userId,
		int currencyId)
	{
		var order = new Order(
			organizationId,
			userId,
			currencyId,
			OrderSource.Manual);

		order.UpdateStatus(OrderStatus.Confirmed);
		return order;
	}

	public static Order CreateOnline(Guid organizationId, Guid userId, int currencyId)
		=> new(organizationId, userId, currencyId, OrderSource.Online);

	public void AddItem(OrderItem item)
	{
		if (item.OrderId != Id)
			throw new InvalidOperationException("Order id miss match");

		_items.Add(item);
	}

	private void UpdateStatus(OrderStatus status)
	{
		Status = status;
	}

	public static Expression<Func<Order, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Order, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public override Guid GetId() => Id;
}
