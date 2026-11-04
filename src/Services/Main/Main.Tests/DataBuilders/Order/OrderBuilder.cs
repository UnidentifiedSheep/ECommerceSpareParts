using Bogus;
using Main.Enums.Orders;
using Tests.Abstractions;
using DomainOrder = Main.Entities.Order.Order;

namespace Tests.DataBuilders.Order;

public sealed class OrderBuilder(Faker faker) : BuilderBase<DomainOrder>(faker)
{
	private readonly List<OrderItemBuilder> _items = [];

	public Guid OrganizationId { get; private set; } = faker.Random.Guid();
	public Guid? UserId { get; private set; }
	public int CurrencyId { get; private set; } = 1;
	public OrderSource Source { get; private set; } = OrderSource.Manual;

	public OrderBuilder WithOrganizationId(Guid organizationId)
	{
		OrganizationId = organizationId;
		return this;
	}

	public OrderBuilder WithUserId(Guid? userId)
	{
		UserId = userId;
		return this;
	}

	public OrderBuilder WithCurrencyId(int currencyId)
	{
		CurrencyId = currencyId;
		return this;
	}

	public OrderBuilder AsOnline(Guid userId)
	{
		Source = OrderSource.Online;
		UserId = userId;
		return this;
	}

	public OrderBuilder WithItems(params OrderItemBuilder[] items)
	{
		_items.AddRange(items);
		return this;
	}

	public override DomainOrder Build()
	{
		var order = Source == OrderSource.Online
			? DomainOrder.CreateOnline(OrganizationId, UserId!.Value, CurrencyId)
			: DomainOrder.CreateManual(OrganizationId, UserId, CurrencyId);
		foreach (var item in _items)
			order.AddItem(item.WithOrder(order).Build());
		return order;
	}
}
