using FluentAssertions;
using Main.Application.Handlers.Characteristics.UpsertCharacteristic;
using Main.Entities;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Characteristics;

public class UpsertCharacteristicTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task WithoutId_CreatesCharacteristic()
	{
		var result = await Mediator.Send(
			new UpsertCharacteristicCommand(null, "  Brake Pads  "),
			CancellationToken);

		result.Characteristic.Id.Should().BeGreaterThan(0);
		result.Characteristic.Name.Should().Be("Brake Pads");
		result.Characteristic.NormalizedName.Should().Be("brake-pads");

		var saved = await Context.Characteristics.AsNoTracking()
			.SingleAsync(CancellationToken);
		saved.Id.Should().Be(result.Characteristic.Id);
		saved.Name.Should().Be(result.Characteristic.Name);
		saved.NormalizedName.Should().Be(result.Characteristic.NormalizedName);
	}

	[Fact]
	public async Task WithId_UpdatesCharacteristic()
	{
		var existing = await new CharacteristicBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(
			new UpsertCharacteristicCommand(existing.Id, "  Oil Filters  "),
			CancellationToken);

		result.Characteristic.Id.Should().Be(existing.Id);
		result.Characteristic.Name.Should().Be("Oil Filters");
		result.Characteristic.NormalizedName.Should().Be("oil-filters");

		var saved = await Context.Characteristics.AsNoTracking()
			.SingleAsync(CancellationToken);
		saved.Id.Should().Be(existing.Id);
		saved.Name.Should().Be("Oil Filters");
		saved.NormalizedName.Should().Be("oil-filters");
	}

	[Fact]
	public async Task WithId_SameNormalizedName_UpdatesWithoutConflict()
	{
		var existing = await new CharacteristicBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(
			new UpsertCharacteristicCommand(existing.Id, "  BRAKE PADS  "),
			CancellationToken);

		result.Characteristic.Id.Should().Be(existing.Id);
		result.Characteristic.Name.Should().Be("BRAKE PADS");
		result.Characteristic.NormalizedName.Should().Be("brake-pads");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task WithoutId_DuplicateNormalizedName_ThrowsConflict()
	{
		await new CharacteristicBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);

		var act = () => Mediator.Send(
			new UpsertCharacteristicCommand(null, "  BRAKE PADS  "),
			CancellationToken);

		var exception = await act.Should().ThrowAsync<CharacteristicNameAlreadyExistsException>();
		exception.Which.LocalizableMessage.MessageKey.Should().Be(
			CharacteristicNormalizedNameAlreadyExistsMessage.Key);
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task WithId_DuplicateOtherCharacteristicName_ThrowsConflictWithoutChangingCharacteristic()
	{
		var existing = await new CharacteristicBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		var other = await new CharacteristicBuilder(Faker)
			.WithName("Oil Filters")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var act = () => Mediator.Send(
			new UpsertCharacteristicCommand(existing.Id, "  OIL FILTERS  "),
			CancellationToken);

		await act.Should().ThrowAsync<CharacteristicNameAlreadyExistsException>();
		var saved = await Context.Characteristics.AsNoTracking()
			.SingleAsync(characteristic => characteristic.Id == existing.Id, CancellationToken);
		saved.Name.Should().Be("Brake Pads");
		saved.NormalizedName.Should().Be("brake-pads");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(2);
		other.Id.Should().NotBe(existing.Id);
	}

	[Fact]
	public async Task WithId_MissingCharacteristic_ThrowsNotFound()
	{
		var act = () => Mediator.Send(
			new UpsertCharacteristicCommand(int.MaxValue, "Brake Pads"),
			CancellationToken);

		await act.Should().ThrowAsync<CharacteristicNotFoundException>();
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("!!!")]
	public async Task InvalidName_ThrowsValidation(string? name)
	{
		var act = () => Mediator.Send(
			new UpsertCharacteristicCommand(null, name!),
			CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task NameOverMaximumLength_ThrowsValidation()
	{
		var act = () => Mediator.Send(
			new UpsertCharacteristicCommand(null, new string('a', 129)),
			CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task NameAtMaximumLength_CreatesCharacteristic()
	{
		var result = await Mediator.Send(
			new UpsertCharacteristicCommand(null, new string('a', 128)),
			CancellationToken);

		result.Characteristic.Name.Should().HaveLength(128);
		result.Characteristic.NormalizedName.Should().HaveLength(128);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task InvalidId_ThrowsValidation(int id)
	{
		var act = () => Mediator.Send(new UpsertCharacteristicCommand(id, "Color"), CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(0);
	}
}
