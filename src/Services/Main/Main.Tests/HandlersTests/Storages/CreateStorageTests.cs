using Tests.Abstractions.Test;
using FluentValidation.TestHelper;
using Main.Entities.Exceptions;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Application.Static;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Storages;

public class CreateStorageTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{

	[Fact]
	public async Task CreateStorage_ExistingCode_ThrowsStorageCodeIsTaken()
	{
		var storageModel = await new StorageBuilder(Faker).BuildAndAddToDb(Context, CancellationToken);

		var command = new CreateStorageCommand(
			storageModel.Code,
			storageModel.Description,
			storageModel.Location,
			storageModel.Type);
		var exception =
			await Assert.ThrowsAsync<DbValidationException>(async () =>
				await Mediator.Send(command, CancellationToken));
		Assert.IsType<StorageCodeAlreadyTakenException>(exception.Failures[0].Error);
	}

	[Fact]
	public async Task CreateStorage_Normal_Succeeds()
	{
		var command = GetCommand();
		await Mediator.Send(command, CancellationToken);

		var createdStorage = await Context.Storages.FirstOrDefaultAsync(
			x => x.Code == command.Code,
			CancellationToken);
		Assert.NotNull(createdStorage);

		Assert.Equal(command.Description, createdStorage.Description);
		Assert.Equal(command.Location, createdStorage.Location);
		Assert.Equal(command.Type, createdStorage.Type);
	}

	private CreateStorageCommand GetCommand()
	{
		var storageModel = new StorageBuilder(Faker).Build();
		return new CreateStorageCommand(
			storageModel.Code,
			storageModel.Description,
			storageModel.Location,
			storageModel.Type);
	}
}

public class CreateStorageValidationTests : ValidationTest<CreateStorageValidation, CreateStorageCommand>
{
	[Fact]
	public async Task CreateStorage_TooLargeCode_FailsValidation()
	{
		var command = new CreateStorageCommand("warehouse", null, null, Main.Enums.StorageType.Warehouse) with { Code = new string('x', 500) };
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.Code).Only();
	}

	[Fact]
	public async Task CreateStorage_TooSmallCode_FailsValidation()
	{
		var command = new CreateStorageCommand("warehouse", null, null, Main.Enums.StorageType.Warehouse) with { Code = "x" };
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.Code).Only();
	}

	[Fact]
	public async Task CreateStorage_TooLargeDescription_FailsValidation()
	{
		var command = new CreateStorageCommand("warehouse", null, null, Main.Enums.StorageType.Warehouse) with { Description = new string('x', 600) };
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.Description).Only();
	}
}
