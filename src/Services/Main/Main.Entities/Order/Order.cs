using Exceptions;
using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Enums.Orders;

namespace Main.Entities.Order;

public class Order : AuditableEntity<Order, Guid>, ILinqEntity<Order, Guid>, IVersionable<uint>
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

	public uint RowVersion { get; private set; }

	private readonly List<OrderItem> _items = [];
	public IReadOnlyList<OrderItem> Items => _items;

	public bool IsPending => Status == OrderStatus.Pending;

	private Order() {}

	private Order(
		Guid organizationId,
		Guid? userId,
		int currencyId,
		OrderSource source)
	{
		Id = Guid.NewGuid();
		OrganizationId = organizationId.EnsureNotEqual(Guid.Empty, OrderInputInvalidMessage.Instance);
		UserId = userId?.EnsureNotEqual(Guid.Empty, OrderInputInvalidMessage.Instance);
		CurrencyId = currencyId.EnsureGreaterThan(0, OrderInputInvalidMessage.Instance);
		Source = source;
		Status = OrderStatus.Pending;
		FulfillmentStatus = OrderFulfillmentStatus.NotAvailable;
	}

	public static Order CreateManual(Guid organizationId, Guid? userId, int currencyId)
		=> new(organizationId, userId, currencyId, OrderSource.Manual);

	public static Order CreateOnline(Guid organizationId, Guid userId, int currencyId)
		=> new(organizationId, userId, currencyId, OrderSource.Online);

	public OrderItem AddItem(int productId, int count)
	{
		if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		var item = OrderItem.Create(Id, productId, count);
		_items.Add(item);

		return item;
	}

	public void RemoveItem(OrderItem item)
	{
		CheckIfBelongs(item);
		EnsureItemsEditable();
		item.Delete();
	}

	public void ChangeCurrency(int currencyId)
	{
		if (CurrencyId == currencyId) return;
		if (!IsPending)
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		CurrencyId = currencyId.EnsureGreaterThan(0, OrderInputInvalidMessage.Instance);
	}

	public void ChangeItemPrice(OrderItem item, decimal? unitPrice)
	{
		CheckIfBelongs(item);
		EnsureItemsEditable();
		item.AssignPrice(null, unitPrice, PriceOrigin.Manual);
	}

	public void ChangeItemCount(OrderItem item, int count)
	{
		CheckIfBelongs(item);

		if (item.Count == count) return;

		EnsureItemsEditable();

		item.SetCount(count);
	}

	public void ChangeItemProduct(OrderItem item, int productId)
	{
		CheckIfBelongs(item);

		if (item.ProductId == productId) return;

		EnsureItemsEditable();

		item.SetProductId(productId);
	}

	public void ChangeOrganization(Guid organizationId, Guid? userId)
	{
		if (OrganizationId == organizationId && UserId == userId)
			return;

		if (Status != OrderStatus.Pending)
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		organizationId.EnsureNotEqual(Guid.Empty, OrderChangeNotAllowedMessage.Instance);

		userId?.EnsureNotEqual(Guid.Empty, OrderChangeNotAllowedMessage.Instance);

		OrganizationId = organizationId;
		UserId = userId;
	}

	public void Confirm(Guid? confirmedBy)
	{
		if (Status != OrderStatus.Pending)
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		Status = OrderStatus.Confirmed;
		ConfirmedAt = DateTime.UtcNow;
		ConfirmedByUserId = confirmedBy;
	}

	public void Complete()
	{
		if (Status != OrderStatus.Confirmed)
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		Status = OrderStatus.Completed;
	}

	public void Cancel()
	{
		if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		Status = OrderStatus.Cancelled;
	}

	public void Delete() => Status = OrderStatus.Deleted;

	public void SetFulfillmentStatus(OrderFulfillmentStatus status)
	{
		if (Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Deleted)
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);

		FulfillmentStatus = status;
	}

	private void EnsureItemsEditable()
	{
		if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
			throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);
	}

	private void CheckIfBelongs(OrderItem item)
	{
		if (!_items.Contains(item) || item.IsDeleted)
			throw new InvalidInputException(OrderItemNotFoundMessage.Instance);
	}


	public static Expression<Func<Order, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Order, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public override Guid GetId() => Id;
}
