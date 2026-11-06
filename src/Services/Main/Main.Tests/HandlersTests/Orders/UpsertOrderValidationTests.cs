using FluentAssertions;
using Main.Application.Orders.Commands.UpsertOrder;
using Main.Application.Orders.Dtos;

namespace Tests.HandlersTests.Orders;

public class UpsertOrderValidationTests
{
	private static UpsertOrderDto Valid => new()
	{
		OrganizationId = Guid.NewGuid(), CurrencyId = 1, RowVersion = null,
		Items = [new() { ProductId = 1, Count = 1 }]
	};

	[Fact]
	public async Task Create_WithExistingItemId_IsRejected()
	{
		var result = await new UpsertOrderValidation().ValidateAsync(new UpsertOrderCommand(Valid with
		{ Items = [new() { Id = 1, ProductId = 1, Count = 1 }] }), TestContext.Current.CancellationToken);
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task Update_WithItemIdAndVersion_IsAccepted()
	{
		var result = await new UpsertOrderValidation().ValidateAsync(new UpsertOrderCommand(Valid with
		{ Id = Guid.NewGuid(), RowVersion = 1, Items = [new() { Id = 1, ProductId = 1, Count = 1 }] }), TestContext.Current.CancellationToken);
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public async Task Update_WithoutVersion_IsRejected()
	{
		var result = await new UpsertOrderValidation().ValidateAsync(new UpsertOrderCommand(Valid with { Id = Guid.NewGuid() }), TestContext.Current.CancellationToken);
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task Update_WithDuplicateItemIds_IsRejected()
	{
		var item = new UpsertOrderItemDto { Id = 1, ProductId = 1, Count = 1 };
		var result = await new UpsertOrderValidation().ValidateAsync(new UpsertOrderCommand(Valid with
		{ Id = Guid.NewGuid(), RowVersion = 1, Items = [item, item] }), TestContext.Current.CancellationToken);
		result.IsValid.Should().BeFalse();
	}
}
