using Exceptions;
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

	private OrderItem(Guid orderId, int productId, int count)
	{
		OrderId = orderId;
		SetProductId(productId);
		SetCount(count);
	}

	internal static OrderItem Create(Guid orderId, int productId, int count)
		=> new(orderId, productId, count);

	internal void AssignPrice(string? signedPrice, decimal? unitPrice, PriceOrigin priceOrigin)
	{
		SignedPrice = signedPrice.TrimSafe();

		UnitPrice = unitPrice?
			.EnsureMaxDecimalPlaces(2, OrderItemInvalidMessage.Instance)
			.EnsureGreaterThan(0, OrderItemInvalidMessage.Instance);

		PriceOrigin = priceOrigin;
	}

	internal void SetCount(int count)
	{
		Count = count.EnsureGreaterThan(0, OrderItemInvalidMessage.Instance);
	}

	internal void SetProductId(int productId)
	{
		ProductId = productId.EnsureGreaterThan(0, OrderItemInvalidMessage.Instance);
	}

	public override int GetId() => Id;
	public static Expression<Func<OrderItem, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<OrderItem, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}
