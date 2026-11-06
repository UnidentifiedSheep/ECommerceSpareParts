using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Order;

using FluentValidation.TestHelper;
using Tests.Abstractions.Test;
using Main.Entities;
using Main.Application.Orders.Commands.UpsertOrder;
using Main.Application.Orders.Dtos;

namespace Tests.HandlersTests.Orders;

public class UpsertOrderTests : IntegrationTest
{
	public UpsertOrderTests(CombinedContainerFixture fixture) : base(fixture) => RegisterBasicContext<OrderTestContext>();
	private OrderTestContext Orders => GetContext<OrderTestContext>();
	private UpsertOrderDto UpdateDto()
	{
		var order = Orders.ManualOrder;
		return new()
		{
			Id = order.Id, RowVersion = order.RowVersion, OrganizationId = order.OrganizationId,
			UserId = order.UserId, CurrencyId = order.CurrencyId,
			Items = order.Items.Select(item => new UpsertOrderItemDto
			{ Id = item.Id, ProductId = item.ProductId, Count = item.Count + 1, UnitPrice = item.UnitPrice }).ToArray()
		};
	}

	[Fact]
	public async Task CreatesNewOrderWithoutPrice()
	{
		var template = UpdateDto();
		var dto = template with
		{
			Id = null, RowVersion = null,
			Items = [new() { ProductId = template.Items.First().ProductId, Count = 2 }]
		};
		var result = await Mediator.Send(new UpsertOrderCommand(dto), CancellationToken);
		var created = await Context.Orders.AsNoTracking().Include(x => x.Items).SingleAsync(x => x.Id == result.OrderId, CancellationToken);
		created.Id.Should().NotBe(template.Id!.Value);
		created.Items.Should().ContainSingle().Which.Count.Should().Be(2);
	}

	[Fact]
	public async Task UpdatesOnlyRequestedOrder()
	{
		var dto = UpdateDto();
		var otherVersion = Orders.OnlineOrder.RowVersion;
		await Mediator.Send(new UpsertOrderCommand(dto), CancellationToken);
		var updated = await Context.Orders.AsNoTracking().Include(x => x.Items).SingleAsync(x => x.Id == dto.Id, CancellationToken);
		updated.Items.Should().ContainSingle().Which.Count.Should().Be(dto.Items.Single().Count);
		(await Context.Orders.AsNoTracking().SingleAsync(x => x.Id == Orders.OnlineOrder.Id, CancellationToken)).RowVersion.Should().Be(otherVersion);
	}

	[Fact]
	public async Task UpdatingPrice_PersistsNewPrice()
	{
		var dto = UpdateDto();
		dto = dto with { Items = [dto.Items.Single() with { UnitPrice = 200m }] };
		await Mediator.Send(new UpsertOrderCommand(dto), CancellationToken);
		(await Context.Orders.AsNoTracking().Include(x => x.Items).SingleAsync(x => x.Id == dto.Id, CancellationToken))
			.Items.Single().UnitPrice.Should().Be(200m);
	}
}

public class UpsertOrderValidationTests : ValidationTest<UpsertOrderValidation, UpsertOrderCommand>
{
	private static UpsertOrderDto Valid => new()
	{
		OrganizationId = Guid.NewGuid(), CurrencyId = 1, RowVersion = null,
		Items = [new() { ProductId = 1, Count = 1 }]
	};

	[Fact]
	public async Task Create_WithExistingItemId_IsRejected()
	{
		var result = await ValidateAsync(new UpsertOrderCommand(Valid with
		{ Items = [new() { Id = 1, ProductId = 1, Count = 1 }] }));
		result.ShouldHaveValidationErrorFor(x => x.Order.Items)
			.WithErrorCode(OrderItemInvalidMessage.Instance.MessageKey).Only();
	}

	[Fact]
	public async Task Update_WithItemIdAndVersion_IsAccepted()
	{
		var result = await ValidateAsync(new UpsertOrderCommand(Valid with
		{ Id = Guid.NewGuid(), RowVersion = 1, Items = [new() { Id = 1, ProductId = 1, Count = 1 }] }));
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public async Task Update_WithoutVersion_IsRejected()
	{
		var result = await ValidateAsync(new UpsertOrderCommand(Valid with { Id = Guid.NewGuid() }));
		result.ShouldHaveValidationErrorFor(x => x.Order.RowVersion)
			.WithErrorCode(OrderRowVersionRequiredMessage.Instance.MessageKey)
			.WithCustomState(OrderRowVersionRequiredMessage.Instance).Only();
	}

	[Fact]
	public async Task Update_WithDuplicateItemIds_IsRejected()
	{
		var item = new UpsertOrderItemDto { Id = 1, ProductId = 1, Count = 1 };
		var result = await ValidateAsync(new UpsertOrderCommand(Valid with
		{ Id = Guid.NewGuid(), RowVersion = 1, Items = [item, item] }));
		result.ShouldHaveValidationErrorFor(x => x.Order.Items)
			.WithErrorCode(OrderItemsInvalidMessage.Instance.MessageKey).Only();
	}
}
