using Tests.Abstractions.Test;
using FluentValidation.TestHelper;
using Abstractions.Models;
using Application.Common.Models;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Storages.EditStorage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Storages;

public class EditStorageTests : IntegrationTest
{
	public EditStorageTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task EditStorage_IsNotSetButHasDescriptionValue_Succeeds()
	{
		var storage = GetStorage();
		var model = new PatchStorageDto
		{
			Description = new PatchField<string?>
			{
				IsSet = false, Value = Faker.Lorem.Sentence(30)
			}
		};
		var command = new EditStorageCommand(storage.Code, model);
		await Mediator.Send(command, CancellationToken);

		var dbStorage = await Context
			.Storages
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Code == storage.Code, CancellationToken);
		Assert.NotNull(dbStorage);

		Assert.Equal(storage.Code, dbStorage.Code);
		Assert.Equal(storage.Description, dbStorage.Description);
		Assert.Equal(storage.Location, dbStorage.Location);
	}

	[Fact]
	public async Task EditStorage_IsNotSetButHasLocationValue_Succeeds()
	{
		var storage = GetStorage();
		var model = new PatchStorageDto
		{
			Location = new PatchField<string?>
			{
				IsSet = false, Value = Faker.Lorem.Sentence(30)
			}
		};
		var command = new EditStorageCommand(storage.Code, model);
		await Mediator.Send(command, CancellationToken);

		var dbStorage = await Context
			.Storages
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Code == storage.Code, CancellationToken);

		Assert.NotNull(dbStorage);

		Assert.Equal(storage.Code, dbStorage.Code);
		Assert.Equal(storage.Description, dbStorage.Description);
		Assert.Equal(storage.Location, dbStorage.Location);
	}

	[Fact]
	public async Task EditStorage_Normal_Succeeds()
	{
		var storage = GetStorage();
		var model = new PatchStorageDto
		{
			Description = new PatchField<string?>
			{
				IsSet = true, Value = Faker.Lorem.Letter(120)
			},
			Location = new PatchField<string?>
			{
				IsSet = true, Value = Faker.Lorem.Letter(120)
			},
			Type = new PatchField<StorageType>
			{
				IsSet = true, Value = StorageType.SupplierStorage
			}
		};
		var command = new EditStorageCommand(storage.Code, model);
		await Mediator.Send(command, CancellationToken);

		var dbStorage = await Context
			.Storages
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Code == storage.Code, CancellationToken);

		Assert.NotNull(dbStorage);
		Assert.Equal(dbStorage.Description, model.Description);
		Assert.Equal(dbStorage.Location, model.Location);
		Assert.Equal(StorageType.SupplierStorage, dbStorage.Type);
	}

	[Fact]
	public async Task EditStorage_InvalidStorageCode_ThrowsStorageNotFound()
	{
		var model = new PatchStorageDto
		{
			Description = new PatchField<string?>
			{
				IsSet = true, Value = Faker.Lorem.Letter(120)
			}
		};
		var command = new EditStorageCommand(Faker.Lorem.Letter(100), model);
		await Assert.ThrowsAsync<StorageNotFoundException>(() => Mediator.Send(command, CancellationToken));
	}

	private Storage GetStorage() =>
		GetContext<StorageTestContext>().Storages.First(x => x.Type == StorageType.Warehouse);
}

public class EditStorageValidationTests : ValidationTest<EditStorageValidation, EditStorageCommand>
{
	[Fact]
	public async Task EditStorage_TooLargeLocation_FailsValidation()
	{
		var storage = new { Code = "warehouse" };
		var model = new PatchStorageDto
		{
			Location = new PatchField<string?>
			{
				IsSet = true, Value = new string('x', 300)
			}
		};
		var command = new EditStorageCommand(storage.Code, model);
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("EditStorage.Location.Value");
	}

	[Fact]
	public async Task EditStorage_TooLargeDesctiption_FailsValidation()
	{
		var storage = new { Code = "warehouse" };
		var model = new PatchStorageDto
		{
			Description = new PatchField<string?>
			{
				IsSet = true, Value = new string('x', 300)
			}
		};
		var command = new EditStorageCommand(storage.Code, model);
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("EditStorage.Description.Value");
	}
}
