using Bogus;
using Main.Entities.Order;
using Main.Enums.Orders;
using Tests.Abstractions;
using DomainOrder = Main.Entities.Order.Order;

namespace Tests.DataBuilders.Order;

public sealed class OrderItemBuilder(Faker faker) : BuilderBase<OrderItem>(faker)
{
	public DomainOrder Order { get; private set; } = DomainOrder.CreateManual(Guid.NewGuid(), null, 1);
	public int ProductId { get; private set; } = faker.Random.Int(1, 10_000);
	public int Count { get; private set; } = 1;
	public decimal? UnitPrice { get; private set; }
	public PriceOrigin PriceOrigin { get; private set; } = PriceOrigin.None;

	public OrderItemBuilder WithOrder(DomainOrder order)
	{
		Order = order;
		return this;
	}

	public OrderItemBuilder WithProductId(int productId)
	{
		ProductId = productId;
		return this;
	}

	public OrderItemBuilder WithCount(int count)
	{
		Count = count;
		return this;
	}

	public OrderItemBuilder WithPrice(decimal unitPrice, PriceOrigin origin = PriceOrigin.Manual)
	{
		UnitPrice = unitPrice;
		PriceOrigin = origin;
		return this;
	}

	public override OrderItem Build()
	{
		var item = OrderItem.Create(Order, ProductId, Count);
		if (UnitPrice.HasValue)
			item.AssignPrice(null, UnitPrice.Value, PriceOrigin);
		return item;
	}
}
