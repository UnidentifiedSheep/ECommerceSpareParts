using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Order;

public sealed class OrderFulfillment : Entity<OrderFulfillment, OrderFulfillmentKey>,
	ILinqEntity<OrderFulfillment, OrderFulfillmentKey>
{

	private OrderFulfillment() { }

	private OrderFulfillment(
		OrderItem orderItem,
		int saleContentId,
		int saleContentProductId,
		int quantity)
	{
		if (orderItem.ProductId != saleContentProductId)
			throw new InvalidOperationException("Products must match.");

		OrderItem = orderItem;
		OrderItemId = orderItem.Id;
		SaleContentId = saleContentId;

		SetCoveredQuantity(quantity);
	}
	public int OrderItemId { get; }
	public int SaleContentId { get; }
	public int CoveredQuantity { get; private set; }

	public OrderItem OrderItem { get; private set; } = null!;
	public static Expression<Func<OrderFulfillment, OrderFulfillmentKey>> GetKeySelector()
		=> x => new OrderFulfillmentKey(x.OrderItemId, x.SaleContentId);
	public static Expression<Func<OrderFulfillment, bool>> GetEqualityExpression(OrderFulfillmentKey key)
		=> x => x.OrderItemId == key.OrderItemId && x.SaleContentId == key.SaleContentId;

	public static OrderFulfillment Create(
		OrderItem orderItem,
		int saleContentId,
		int saleContentProductId,
		int quantity)
		=> new(
			orderItem,
			saleContentId,
			saleContentProductId,
			quantity);

	private void SetCoveredQuantity(int coveredQuantity)
		=> CoveredQuantity = coveredQuantity.EnsureGreaterThan(
			0,
			() => new InvalidOperationException("Coverage quantity must be greater than 0."));

	public override OrderFulfillmentKey GetId() => new(OrderItemId, SaleContentId);
}

public readonly record struct OrderFulfillmentKey(int OrderItemId, int SaleContentId) : ICompositeKey
{
	public object[] ToArray() => [OrderItemId, SaleContentId];
}
