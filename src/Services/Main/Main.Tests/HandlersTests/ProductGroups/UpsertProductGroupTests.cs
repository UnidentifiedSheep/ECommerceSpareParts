using FluentAssertions;
using Main.Application.Handlers.ProductGroups.UpsertProductGroup;
using Main.Entities;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.ProductGroups;

public class UpsertProductGroupTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task WithoutId_CreatesGroup()
	{
		var result = await Mediator.Send(
			new UpsertProductGroupCommand(null, "  Brake Pads  "),
			CancellationToken);

		result.Group.Id.Should().BeGreaterThan(0);
		result.Group.Name.Should().Be("Brake Pads");
		result.Group.NormalizedName.Should().Be("brake-pads");

		var saved = await Context.ProductGroups.AsNoTracking()
			.SingleAsync(CancellationToken);
		saved.Id.Should().Be(result.Group.Id);
		saved.Name.Should().Be(result.Group.Name);
		saved.NormalizedName.Should().Be(result.Group.NormalizedName);
	}

	[Fact]
	public async Task WithId_UpdatesGroup()
	{
		var existing = await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(
			new UpsertProductGroupCommand(existing.Id, "  Oil Filters  "),
			CancellationToken);

		result.Group.Id.Should().Be(existing.Id);
		result.Group.Name.Should().Be("Oil Filters");
		result.Group.NormalizedName.Should().Be("oil-filters");

		var saved = await Context.ProductGroups.AsNoTracking()
			.SingleAsync(CancellationToken);
		saved.Id.Should().Be(existing.Id);
		saved.Name.Should().Be("Oil Filters");
		saved.NormalizedName.Should().Be("oil-filters");
	}

	[Fact]
	public async Task WithId_SameNormalizedName_UpdatesWithoutConflict()
	{
		var existing = await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(
			new UpsertProductGroupCommand(existing.Id, "  BRAKE PADS  "),
			CancellationToken);

		result.Group.Id.Should().Be(existing.Id);
		result.Group.Name.Should().Be("BRAKE PADS");
		result.Group.NormalizedName.Should().Be("brake-pads");
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task WithoutId_DuplicateNormalizedName_ThrowsConflict()
	{
		await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);

		var act = () => Mediator.Send(
			new UpsertProductGroupCommand(null, "  BRAKE PADS  "),
			CancellationToken);

		var exception = await act.Should().ThrowAsync<ProductGroupNameAlreadyExistsException>();
		exception.Which.LocalizableMessage.MessageKey.Should().Be(
			ProductGroupNormalizedNameAlreadyExistsMessage.Key);
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task WithId_DuplicateOtherGroupName_ThrowsConflictWithoutChangingGroup()
	{
		var existing = await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);
		var other = await new ProductGroupBuilder(Faker)
			.WithName("Oil Filters")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var act = () => Mediator.Send(
			new UpsertProductGroupCommand(existing.Id, "  OIL FILTERS  "),
			CancellationToken);

		await act.Should().ThrowAsync<ProductGroupNameAlreadyExistsException>();
		var saved = await Context.ProductGroups.AsNoTracking()
			.SingleAsync(group => group.Id == existing.Id, CancellationToken);
		saved.Name.Should().Be("Brake Pads");
		saved.NormalizedName.Should().Be("brake-pads");
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(2);
		other.Id.Should().NotBe(existing.Id);
	}

	[Fact]
	public async Task WithId_MissingGroup_ThrowsNotFound()
	{
		var act = () => Mediator.Send(
			new UpsertProductGroupCommand(int.MaxValue, "Brake Pads"),
			CancellationToken);

		await act.Should().ThrowAsync<ProductGroupNotFoundException>();
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("ab")]
	[InlineData("!!!")]
	public async Task InvalidName_ThrowsValidation(string name)
	{
		var act = () => Mediator.Send(
			new UpsertProductGroupCommand(null, name),
			CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(0);
	}
}
