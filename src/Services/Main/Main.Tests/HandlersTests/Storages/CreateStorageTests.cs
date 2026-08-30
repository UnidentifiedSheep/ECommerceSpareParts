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
	public async Task CreateStorage_TooLargeCode_FailsValidation()
	{
		var command = GetCommand() with
		{
			Code = Faker.Lorem.Letter(500)
		};
		await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task CreateStorage_TooSmallCode_FailsValidation()
	{
		var command = GetCommand() with
		{
			Code = Faker.Lorem.Letter()
		};
		await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task CreateStorage_TooLargeDescription_FailsValidation()
	{
		var command = GetCommand() with
		{
			Description = Faker.Lorem.Letter(600)
		};
		await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task CreateStorage_ExistingCode_ThrowsStorageCodeIsTaken()
	{
		var storageModel = await new StorageBuilder(Faker).BuildAndAddToDb(Context);

		var command = new CreateStorageCommand(
			storageModel.Code,
			storageModel.Description,
			storageModel.Location,
			storageModel.Type);
		var exception =
			await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
		Assert.Equal(ApplicationErrors.StoragesCodeAlreadyTaken, exception.Failures[0].ErrorName);
	}

	[Fact]
	public async Task CreateStorage_Normal_Succeeds()
	{
		var command = GetCommand();
		await Mediator.Send(command);

		var createdStorage = await Context.Storages.FirstOrDefaultAsync(x => x.Code == command.Code);
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
