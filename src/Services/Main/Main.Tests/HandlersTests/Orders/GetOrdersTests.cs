using Application.Common.Models;
using Application.Common.Querying;
using Enums;
using Exceptions;
using FluentAssertions;
using FluentValidation;
using Main.Application.Orders.Queries.GetOrders;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Enums.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Order;
using DomainOrder = Main.Entities.Order.Order;
using Product = Main.Entities.Product.Product;

namespace Tests.HandlersTests.Orders;

public sealed class GetOrdersTests : IntegrationTest
{
	public GetOrdersTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<OrderTestContext>();
	}

	private OrderTestContext TestContext => GetContext<OrderTestContext>();
	private UserContextMock UserContext => (UserContextMock)Scope.ServiceProvider.GetRequiredService<IUserContext>();
	private DomainOrder[] Orders => [TestContext.OnlineOrder, TestContext.ManualOrder, TestContext.EmptyOrder, TestContext.ConfirmedOrder];

	[Fact]
	public async Task AllPermission_ReturnsAllOrdersIncludingUnassigned()
	{
		var result = await Search();

		result.Orders.Select(order => order.Id).Should().BeEquivalentTo(Orders.Select(order => order.Id));
	}

	[Fact]
	public async Task OwnPermission_ReturnsOnlyCurrentUsersOrders()
	{
		var result = await Search(permission: PermissionCodes.ORDERS_GET_OWN);

		result.Orders.Should().ContainSingle(order => order.Id == TestContext.OnlineOrder.Id);
	}

	[Fact]
	public async Task OwnPermission_UserFilterCannotExposeAnotherUsersOrders()
	{
		var result = await Search(Query() with { UserIds = [TestContext.OtherUser.Id] }, PermissionCodes.ORDERS_GET_OWN);

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task OwnPermission_SearchCannotExposeAnotherUsersOrder()
	{
		var result = await Search(Query() with { SearchTerm = TestContext.ConfirmedOrder.Id.ToString() }, PermissionCodes.ORDERS_GET_OWN);

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task BothPermissions_AllPermissionTakesPrecedence()
	{
		UserContext.SetPermissions([Permission.ToNormalizedPermission(PermissionCodes.ORDERS_GET_OWN)]);

		var result = await Search();

		result.Orders.Should().HaveCount(Orders.Length);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task MissingPermissionOrAuthentication_DeniesAccess(bool authenticated)
	{
		UserContext.SetIsAuthenticated(authenticated);

		await Assert.ThrowsAsync<OrderReadAccessDeniedException>(() =>
			Search(permission: authenticated ? null : PermissionCodes.ORDERS_GET));
	}

	[Theory]
	[InlineData("organization")]
	[InlineData("user")]
	[InlineData("currency")]
	[InlineData("product")]
	[InlineData("status")]
	[InlineData("fulfillment")]
	public async Task Filters_SelectMatchingOrders(string filter)
	{
		var order = TestContext.ConfirmedOrder;
		var query = filter switch
		{
			"organization" => Query() with { OrganizationIds = [order.OrganizationId] },
			"user" => Query() with { UserIds = [order.UserId!.Value] },
			"currency" => Query() with { CurrencyIds = [order.CurrencyId] },
			"product" => Query() with { ProductIds = [order.Items[0].ProductId] },
			"status" => Query() with { Statuses = [OrderStatus.Confirmed] },
			_ => Query() with { FulfillmentStatuses = [OrderFulfillmentStatus.FullyAvailable] }
		};

		var result = await Search(query);

		result.Orders.Should().ContainSingle(item => item.Id == order.Id);
	}

	[Fact]
	public async Task SourceFilter_ReturnsOnlineOrders()
	{
		var result = await Search(Query() with { Sources = [OrderSource.Online] });

		result.Orders.Should().ContainSingle(order => order.Id == TestContext.OnlineOrder.Id);
	}

	[Fact]
	public async Task DifferentFilters_AreCombinedWithAnd()
	{
		var result = await Search(Query() with { Sources = [OrderSource.Online], Statuses = [OrderStatus.Confirmed] });

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task MultipleProducts_DoNotDuplicateOrders()
	{
		var result = await Search(Query() with { ProductIds = TestContext.OnlineOrder.Items.Select(item => item.ProductId).ToArray() });

		result.Orders.Select(order => order.Id).Should().BeEquivalentTo([TestContext.OnlineOrder.Id, TestContext.ManualOrder.Id]);
	}

	[Fact]
	public async Task CreatedAtRange_IsInclusive()
	{
		var start = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
		await Context.Set<DomainOrder>().ExecuteUpdateAsync(set => set.SetProperty(order => order.CreatedAt, start.AddDays(-1)), CancellationToken);
		await Context.Set<DomainOrder>().Where(order => order.Id == TestContext.OnlineOrder.Id)
			.ExecuteUpdateAsync(set => set.SetProperty(order => order.CreatedAt, start), CancellationToken);
		await Context.Set<DomainOrder>().Where(order => order.Id == TestContext.ConfirmedOrder.Id)
			.ExecuteUpdateAsync(set => set.SetProperty(order => order.CreatedAt, start.AddDays(1)), CancellationToken);

		var result = await Search(Query() with { DateRange = new(start, start.AddDays(1)) });

		result.Orders.Select(order => order.Id).Should().BeEquivalentTo([TestContext.OnlineOrder.Id, TestContext.ConfirmedOrder.Id]);
	}

	[Theory]
	[InlineData("organization")]
	[InlineData("user")]
	[InlineData("name")]
	[InlineData("sku")]
	[InlineData("id")]
	public async Task SearchTerm_FindsOrderThroughRelatedData(string field)
	{
		var order = TestContext.ConfirmedOrder;
		var product = await Context.Set<Product>().SingleAsync(item => item.Id == order.Items[0].ProductId, CancellationToken);
		product.SetName("Order search pump");
		product.SetSku("RP-00999");
		await Context.SaveChangesAsync(CancellationToken);
		var term = field switch
		{
			"organization" => "pArTnEr PaRtS",
			"user" => "ORDER-BUYER",
			"name" => "search pump",
			"sku" => "r p 00999",
			_ => order.Id.ToString()
		};

		var result = await Search(Query() with { SearchTerm = term });

		result.Orders.Should().ContainSingle(item => item.Id == order.Id);
	}

	[Theory]
	[InlineData("%")]
	[InlineData("_")]
	[InlineData("\\")]
	public async Task SearchTerm_TreatsWildcardsLiterally(string term)
	{
		TestContext.OtherOrganization.SetName("Partner %_\\ Parts");
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Search(Query() with { SearchTerm = term });

		result.Orders.Should().ContainSingle(order => order.Id == TestContext.ConfirmedOrder.Id);
	}

	[Fact]
	public async Task DefaultSorting_IsNewestFirst()
	{
		var result = await Search();

		result.Orders.Select(order => order.Id).Should().Equal(Orders.OrderByDescending(order => order.CreatedAt).ThenBy(order => order.Id).Select(order => order.Id));
	}

	[Fact]
	public async Task Pagination_WithEqualDatesUsesIdAsStableTieBreaker()
	{
		var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		await Context.Set<DomainOrder>().ExecuteUpdateAsync(set => set.SetProperty(order => order.CreatedAt, date), CancellationToken);

		var first = await Search(Query() with { Pagination = new(0, 2) });
		var second = await Search(Query() with { Pagination = new(1, 2) });

		first.Orders.Concat(second.Orders).Select(order => order.Id).Should().Equal(Orders.Select(order => order.Id).Order());
	}

	[Theory]
	[InlineData("status")]
	[InlineData("fulfillmentStatus")]
	[InlineData("source")]
	[InlineData("confirmedAt")]
	[InlineData("updatedAt")]
	public async Task ExplicitSorting_IsApplied(string field)
	{
		if (field == "status")
		{
			TestContext.ManualOrder.Confirm(null);
			TestContext.ManualOrder.Complete();
			TestContext.EmptyOrder.Cancel();
			await Context.SaveChangesAsync(CancellationToken);
		}
		if (field == "fulfillmentStatus")
		{
			TestContext.ManualOrder.SetFulfillmentStatus(OrderFulfillmentStatus.PartiallyAvailable);
			await Context.SaveChangesAsync(CancellationToken);
		}

		var result = await Search(Query() with { SortBy = [new SortExpression(field)] });

		var expected = field switch
		{
			"status" => Orders.OrderBy(order => order.Status).ThenBy(order => order.Id),
			"fulfillmentStatus" => Orders.OrderBy(order => order.FulfillmentStatus).ThenBy(order => order.Id),
			"source" => Orders.OrderBy(order => order.Source).ThenBy(order => order.Id),
			"confirmedAt" => Orders.OrderBy(order => order.ConfirmedAt == null).ThenBy(order => order.ConfirmedAt).ThenBy(order => order.Id),
			_ => Orders.OrderBy(order => order.UpdatedAt).ThenBy(order => order.Id)
		};
		result.Orders.Select(order => order.Id).Should().Equal(expected.Select(order => order.Id));
	}

	[Theory]
	[InlineData("pagination")]
	[InlineData("range")]
	[InlineData("term")]
	[InlineData("status")]
	[InlineData("fulfillment")]
	[InlineData("source")]
	public async Task InvalidInput_FailsValidation(string input)
	{
		var query = input switch
		{
			"pagination" => Query() with { Pagination = new(-1, 10) },
			"range" => Query() with { DateRange = new(DateTime.UtcNow.AddDays(1), DateTime.UtcNow) },
			"term" => Query() with { SearchTerm = new string('x', 201) },
			"status" => Query() with { Statuses = [(OrderStatus)999] },
			"fulfillment" => Query() with { FulfillmentStatuses = [(OrderFulfillmentStatus)999] },
			_ => Query() with { Sources = [(OrderSource)999] }
		};

		await Assert.ThrowsAsync<ValidationException>(() => Search(query));
	}

	[Fact]
	public async Task InvalidSorting_ThrowsInvalidInputException()
	{
		await Assert.ThrowsAsync<InvalidInputException>(() => Search(Query() with { SortBy = [new SortExpression("unknown")] }));
	}

	private static GetOrdersQuery Query() => new(new(), new(0, 100), [], [], [], [], [], [], [], [], null);

	private Task<GetOrdersResult> Search(GetOrdersQuery? query = null, PermissionCodes? permission = PermissionCodes.ORDERS_GET)
	{
		if (permission.HasValue)
			UserContext.SetPermissions([Permission.ToNormalizedPermission(permission.Value)]);
		return Mediator.Send(query ?? Query(), CancellationToken);
	}
}
