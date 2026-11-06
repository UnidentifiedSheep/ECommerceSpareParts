using FluentValidation.TestHelper;
using Tests.Abstractions.Test;
using Main.Entities;
using Main.Application.Orders.Commands.UpsertOrder;
using Main.Application.Orders.Dtos;

namespace Tests.HandlersTests.Orders;

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
