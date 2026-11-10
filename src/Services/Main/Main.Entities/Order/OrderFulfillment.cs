using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.Sale;

namespace Main.Entities.Order;

public sealed class OrderFulfillment : Entity<OrderFulfillment, OrderFulfillmentKey>, ILinqEntity<OrderFulfillment, OrderFulfillmentKey>
{
	public int OrderItemId { get; private set; }
	public int SaleContentId { get; private set; }
	public int CoveredQuantity { get; private set; }

	public OrderItem OrderItem { get; private set; } = null!;
	public SaleContent SaleContent { get; private set; } = null!;

	private OrderFulfillment() {}

	private OrderFulfillment(
		OrderItem orderItem,
		SaleContent saleContent,
		int quantity)
	{
		if (orderItem.ProductId != saleContent.ProductId)
			throw new InvalidOperationException("Products must match.");

		OrderItem = orderItem;
		OrderItemId = orderItem.Id;

		SaleContent = saleContent;
		SaleContentId = saleContent.Id;

		SetCoveredQuantity(quantity);
	}

	public static OrderFulfillment Create(
		OrderItem orderItem,
		SaleContent saleContent,
		int quantity)
		=> new(orderItem, saleContent, quantity);

	private void SetCoveredQuantity(int coveredQuantity)
	{
		CoveredQuantity = coveredQuantity.EnsureGreaterThan(
			min: 0,
			exceptionFactory: () => new InvalidOperationException("Coverage quantity must be greater than 0."));
	}

	public override OrderFulfillmentKey GetId() => new(OrderItemId, SaleContentId);
	public static Expression<Func<OrderFulfillment, OrderFulfillmentKey>> GetKeySelector()
		=> x => new OrderFulfillmentKey(x.OrderItemId, x.SaleContentId);
	public static Expression<Func<OrderFulfillment, bool>> GetEqualityExpression(OrderFulfillmentKey key)
		=> x => x.OrderItemId == key.OrderItemId && x.SaleContentId == key.SaleContentId;
}

public readonly record struct OrderFulfillmentKey(int OrderItemId, int SaleContentId) : ICompositeKey
{
	public object[] ToArray() => [OrderItemId, SaleContentId];
}
