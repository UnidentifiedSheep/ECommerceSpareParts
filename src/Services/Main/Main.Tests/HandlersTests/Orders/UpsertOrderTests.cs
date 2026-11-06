using Microsoft.Extensions.DependencyInjection;
using Exceptions;
using Main.Entities.Exceptions;
using Main.Enums.Orders;
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
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ReplacesItems_SoftDeletesMissingUpdatesRetainedAndAddsNew(bool confirmed)
	{
		var order = Orders.OnlineOrder;
		if (confirmed)
		{
			order.Confirm(order.UserId);
			await Context.SaveChangesAsync(CancellationToken);
		}
		var retained = order.Items[0];
		var removed = order.Items[1];
		var otherItemIds = await Context.OrderItems.AsNoTracking().Where(item => item.OrderId != order.Id)
			.Select(item => item.Id).ToArrayAsync(CancellationToken);
		var dto = new UpsertOrderDto
		{
			Id = order.Id, RowVersion = order.RowVersion, OrganizationId = order.OrganizationId,
			UserId = order.UserId, CurrencyId = order.CurrencyId,
			Items = [
				new() { Id = retained.Id, ProductId = retained.ProductId, Count = retained.Count + 1, UnitPrice = retained.UnitPrice },
				new() { ProductId = removed.ProductId, Count = 7 }]
		};

		await Mediator.Send(new UpsertOrderCommand(dto), CancellationToken);

		var saved = await Context.OrderItems.AsNoTracking().Where(item => item.OrderId == order.Id && !item.IsDeleted).ToArrayAsync(CancellationToken);
		saved.Should().HaveCount(2);
		saved.Single(item => item.Id == retained.Id).Count.Should().Be(dto.Items.First().Count);
		saved.Single(item => item.Id != retained.Id).Count.Should().Be(7);
		(await Context.OrderItems.AsNoTracking().SingleAsync(item => item.Id == removed.Id, CancellationToken)).IsDeleted.Should().BeTrue();
		(await Context.OrderItems.AsNoTracking().Where(item => item.OrderId != order.Id)
			.Select(item => item.Id).ToArrayAsync(CancellationToken)).Should().BeEquivalentTo(otherItemIds);
	}

	[Theory]
	[InlineData(OrderStatus.Completed)]
	[InlineData(OrderStatus.Cancelled)]
	[InlineData(OrderStatus.Deleted)]
	public async Task RemovingItemFromClosedOrder_IsRejectedWithoutDeletingItems(OrderStatus status)
	{
		var order = Orders.OnlineOrder;
		if (status == OrderStatus.Completed)
		{
			order.Confirm(order.UserId);
			order.Complete();
		}
		else if (status == OrderStatus.Deleted) order.Delete();
		else order.Cancel();
		await Context.SaveChangesAsync(CancellationToken);
		var retained = order.Items[0];
		var dto = new UpsertOrderDto
		{
			Id = order.Id, RowVersion = order.RowVersion, OrganizationId = order.OrganizationId,
			UserId = order.UserId, CurrencyId = order.CurrencyId,
			Items = [new() { Id = retained.Id, ProductId = retained.ProductId, Count = retained.Count, UnitPrice = retained.UnitPrice }]
		};

		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(new UpsertOrderCommand(dto), CancellationToken));

		(await Context.OrderItems.AsNoTracking().CountAsync(item => item.OrderId == order.Id, CancellationToken)).Should().Be(2);
	}

	[Fact]
	public async Task ItemFromAnotherOrder_IsRejectedWithoutDeletingItems()
	{
		var dto = UpdateDto();
		dto = dto with { Items = [dto.Items.Single() with { Id = Orders.OnlineOrder.Items[0].Id }] };
		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(new UpsertOrderCommand(dto), CancellationToken));
		(await Context.OrderItems.AsNoTracking().CountAsync(item => item.OrderId == dto.Id, CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task SoftDeletedItems_AreNotReturnedAndCannotBeUpdated()
	{
		var order = Orders.OnlineOrder;
		var removed = order.Items[1];
		order.RemoveItem(removed);
		await Context.SaveChangesAsync(CancellationToken);
		var userContext = (global::Tests.Stubs.UserContextMock)Scope.ServiceProvider
			.GetRequiredService<Security.Core.Interfaces.IUserContext>();
		userContext.SetIsAuthenticated(true).SetUserId(order.UserId!.Value)
			.SetPermissions([Main.Entities.Auth.Permission.ToNormalizedPermission(Enums.PermissionCodes.ORDERS_GET)]);

		var result = await Mediator.Send(new Main.Application.Orders.Queries.GetOrderItemsByOrderIdsQuery(order.Id), CancellationToken);
		result.Items[order.Id].Should().ContainSingle().Which.Id.Should().NotBe(removed.Id);

		var dto = new UpsertOrderDto
		{
			Id = order.Id, RowVersion = order.RowVersion, OrganizationId = order.OrganizationId,
			UserId = order.UserId, CurrencyId = order.CurrencyId,
			Items = [new() { Id = removed.Id, ProductId = removed.ProductId, Count = removed.Count, UnitPrice = removed.UnitPrice }]
		};
		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(new UpsertOrderCommand(dto), CancellationToken));
		(await Context.OrderItems.AsNoTracking().SingleAsync(item => item.Id == removed.Id, CancellationToken))
			.IsDeleted.Should().BeTrue();
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
		{ Id = Guid.NewGuid(), RowVersion = 1, Items = [item, item with { ProductId = 2, Count = 3 }] }));
		result.ShouldHaveValidationErrorFor(x => x.Order.Items)
			.WithErrorCode(OrderItemsInvalidMessage.Instance.MessageKey).Only();
	}
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task SeveralNewItemsWithoutIds_AreAccepted(bool updating)
	{
		var dto = Valid with
		{
			Id = updating ? Guid.NewGuid() : null, RowVersion = updating ? 1u : null,
			Items = [new() { ProductId = 1, Count = 1 }, new() { ProductId = 2, Count = 2 }]
		};
		var result = await ValidateAsync(new UpsertOrderCommand(dto));
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public async Task DifferentExistingIdsMixedWithNewItems_AreAccepted()
	{
		var dto = Valid with
		{
			Id = Guid.NewGuid(), RowVersion = 1,
			Items = [new() { Id = 1, ProductId = 1, Count = 1 }, new() { Id = 2, ProductId = 2, Count = 2 }, new() { ProductId = 3, Count = 1 }]
		};
		var result = await ValidateAsync(new UpsertOrderCommand(dto));
		result.ShouldNotHaveAnyValidationErrors();
	}

}
