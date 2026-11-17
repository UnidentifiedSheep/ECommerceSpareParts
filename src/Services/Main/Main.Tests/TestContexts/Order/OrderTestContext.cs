using Main.Entities.Order.Enums.Orders;
using Main.Entities.Organization.Organization;
using Main.Entities.User;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Order;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;
using Tests.TestContexts.Currency;
using DomainOrder = Main.Entities.Order.Order.Order;

namespace Tests.TestContexts.Order;

public sealed class OrderTestContext(
	UnifiedDbContext context,
	UserContextTestContext userContext,
	CurrencyTestContext currencies,
	ProductTestContext products) : TestContextBase<UnifiedDbContext>(context), IDependentTestContext
{
	public DomainOrder OnlineOrder { get; private set; } = null!;
	public DomainOrder ManualOrder { get; private set; } = null!;
	public DomainOrder EmptyOrder { get; private set; } = null!;
	public DomainOrder ConfirmedOrder { get; private set; } = null!;
	public User OtherUser { get; private set; } = null!;
	public Organization OtherOrganization { get; private set; } = null!;

	public static Type[] DependsOn { get; } =
	[
		typeof(UserContextTestContext), typeof(CurrencyTestContext), typeof(ProductTestContext)
	];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var organizationId = userContext.SystemUser.Id;
		var currencyId = currencies.Currencies[0].Id;
		OnlineOrder = await new OrderBuilder(Faker)
			.WithOrganizationId(organizationId)
			.WithCurrencyId(currencyId)
			.AsOnline(userContext.SystemUser.Id)
			.WithItems(
				new OrderItemBuilder(Faker).WithProductId(products.Products[0].Id)
					.WithCount(2)
					.WithPrice(125.50m, PriceOrigin.PricingService),
				new OrderItemBuilder(Faker).WithProductId(products.Products[1].Id).WithCount(3))
			.BuildAndAddToDb(DbContext, cancellationToken);

		ManualOrder = await new OrderBuilder(Faker)
			.WithOrganizationId(organizationId)
			.WithCurrencyId(currencyId)
			.WithItems(
				new OrderItemBuilder(Faker).WithProductId(products.Products[0].Id)
					.WithCount(1)
					.WithPrice(90m))
			.BuildAndAddToDb(DbContext, cancellationToken);

		EmptyOrder = await new OrderBuilder(Faker)
			.WithOrganizationId(organizationId)
			.WithCurrencyId(currencyId)
			.BuildAndAddToDb(DbContext, cancellationToken);

		OtherUser = await new UserBuilder(Faker).WithUserName("order-buyer")
			.BuildAndAddToDb(DbContext, cancellationToken);
		OtherOrganization = await new OrganizationBuilder(Faker).WithOwnerId(OtherUser.Id)
			.WithName("Partner Parts")
			.BuildAndAddToDb(DbContext, cancellationToken);
		ConfirmedOrder = new OrderBuilder(Faker)
			.WithOrganizationId(OtherOrganization.Id)
			.WithCurrencyId(currencies.Currencies[1].Id)
			.WithUserId(OtherUser.Id)
			.WithItems(
				new OrderItemBuilder(Faker).WithProductId(products.Products[2].Id)
					.WithCount(4)
					.WithPrice(80m))
			.Build();
		ConfirmedOrder.Confirm(userContext.SystemUser.Id);
		ConfirmedOrder.SetFulfillmentStatus(OrderFulfillmentStatus.FullyAvailable);
		await DbContext.AddAsync(ConfirmedOrder, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
