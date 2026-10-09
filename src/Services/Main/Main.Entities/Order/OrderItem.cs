using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Main.Enums.Orders;

namespace Main.Entities.Order;

public class OrderItem : Entity<OrderItem, int>, ILinqEntity<OrderItem, int>
{
	public int Id { get; private set; }
	public Guid OrderId { get; private set; }
	public int ProductId { get; private set; }
	public int Count { get; private set; }

	public decimal? UnitPrice { get; private set; }
	public string? SignedPrice { get; private set; }
	public PriceOrigin PriceOrigin { get; private set; }

	private OrderItem() {}

	private OrderItem(Order order, int productId, int count)
	{
		OrderId = order.Id;
		ProductId = productId;
		Count = count;
	}

	public static OrderItem Create(Order order, int productId, int count)
		=> new(order, productId, count);

	public void AssignPrice(string? signedPrice, decimal unitPrice, PriceOrigin priceOrigin)
	{
		SignedPrice = signedPrice.TrimSafe();

		UnitPrice = unitPrice
			.EnsureMaxDecimalPlaces(
				maxDecimals: 2,
				exceptionFactory: () => new InvalidOperationException("Price must have maximum 2 decimal places."))
			.EnsureGreaterThan(
				min: 0,
				exceptionFactory: () => new InvalidOperationException("Price must be greater than zero."));

		PriceOrigin = priceOrigin;
	}

	public override int GetId() => Id;
	public static Expression<Func<OrderItem, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<OrderItem, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}
