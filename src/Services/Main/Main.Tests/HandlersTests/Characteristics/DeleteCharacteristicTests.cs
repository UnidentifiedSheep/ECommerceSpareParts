using FluentAssertions;
using Main.Application.Handlers.Characteristics.DeleteCharacteristic;
using Main.Entities;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Characteristics;

public class DeleteCharacteristicTests : IntegrationTest
{
	public DeleteCharacteristicTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	[Fact]
	public async Task ExistingCharacteristic_RemovesOnlyRequestedCharacteristic()
	{
		var existing = await new CharacteristicBuilder(Faker).WithName("Color").BuildAndAddToDb(Context);
		var other = await new CharacteristicBuilder(Faker).WithName("Size").BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		await Mediator.Send(new DeleteCharacteristicCommand(existing.Id), CancellationToken);

		var remaining = await Context.Characteristics.AsNoTracking().ToListAsync(CancellationToken);
		remaining.Should().ContainSingle().Which.Id.Should().Be(other.Id);
	}

	[Fact]
	public async Task MissingCharacteristic_ThrowsNotFound()
	{
		var act = () => Mediator.Send(new DeleteCharacteristicCommand(int.MaxValue), CancellationToken);

		var exception = await act.Should().ThrowAsync<CharacteristicNotFoundException>();
		exception.Which.LocalizableMessage.MessageKey.Should().Be(CharacteristicNotFoundMessage.Key);
	}

	[Fact]
	public async Task CharacteristicUsedByProduct_ThrowsConflictAndPreservesData()
	{
		var existing = await new CharacteristicBuilder(Faker).WithName("Color").BuildAndAddToDb(Context);
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var link = ProductCharacteristic.Create(productId, existing.Id, "Red");
		await Context.ProductCharacteristics.AddAsync(link, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var act = () => Mediator.Send(new DeleteCharacteristicCommand(existing.Id), CancellationToken);

		var exception = await act.Should().ThrowAsync<CharacteristicInUseException>();
		exception.Which.LocalizableMessage.MessageKey.Should().Be(CharacteristicInUseMessage.Key);
		(await Context.Characteristics.AnyAsync(x => x.Id == existing.Id, CancellationToken)).Should().BeTrue();
		(await Context.ProductCharacteristics.AnyAsync(
			x => x.ProductId == productId && x.CharacteristicId == existing.Id, CancellationToken)).Should().BeTrue();
	}

	[Fact]
	public async Task CharacteristicUsedByProduct_DoesNotPreventDeletingAnotherCharacteristic()
	{
		var used = await new CharacteristicBuilder(Faker).WithName("Color").BuildAndAddToDb(Context);
		var unused = await new CharacteristicBuilder(Faker).WithName("Size").BuildAndAddToDb(Context);
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		await Context.ProductCharacteristics.AddAsync(
			ProductCharacteristic.Create(productId, used.Id, "Red"), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		await Mediator.Send(new DeleteCharacteristicCommand(unused.Id), CancellationToken);

		var remaining = await Context.Characteristics.AsNoTracking().ToListAsync(CancellationToken);
		remaining.Should().ContainSingle().Which.Id.Should().Be(used.Id);
		(await Context.ProductCharacteristics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task InvalidId_ThrowsValidation(int id)
	{
		var act = () => Mediator.Send(new DeleteCharacteristicCommand(id), CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
	}
}
