using Abstractions.Models;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Storages.EditStorage;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Storages;

public class EditStorageTests : Test
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
                IsSet = false,
                Value = Faker.Lorem.Sentence(30)
            }
        };
        var command = new EditStorageCommand(storage.Name, model);
        await Mediator.Send(command);
        
        var dbStorage = await Context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == storage.Name);
        Assert.NotNull(dbStorage);

        Assert.Equal(storage.Name, dbStorage.Name);
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
                IsSet = false,
                Value = Faker.Lorem.Sentence(30)
            }
        };
        var command = new EditStorageCommand(storage.Name, model);
        await Mediator.Send(command);

        var dbStorage = await Context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == storage.Name);
        
        Assert.NotNull(dbStorage);

        Assert.Equal(storage.Name, dbStorage.Name);
        Assert.Equal(storage.Description, dbStorage.Description);
        Assert.Equal(storage.Location, dbStorage.Location);
    }

    [Fact]
    public async Task EditStorage_TooLargeLocation_FailsValidation()
    {
        var storage = GetStorage();
        var model = new PatchStorageDto
        {
            Location = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(300)
            }
        };
        var command = new EditStorageCommand(storage.Name, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorage_TooLargeDesctiption_FailsValidation()
    {
        var storage = GetStorage();
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(300)
            }
        };
        var command = new EditStorageCommand(storage.Name, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorage_Normal_Succeeds()
    {
        var storage = GetStorage();
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(120)
            },
            Location = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(120)
            },
            Type = new PatchField<StorageType>
            {
                IsSet = true,
                Value = StorageType.SupplierStorage
            }
        };
        var command = new EditStorageCommand(storage.Name, model);
        await Mediator.Send(command);

        var dbStorage = await Context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == storage.Name);
        
        Assert.NotNull(dbStorage);
        Assert.Equal(dbStorage.Description, model.Description);
        Assert.Equal(dbStorage.Location, model.Location);
        Assert.Equal(StorageType.SupplierStorage, dbStorage.Type);
    }

    [Fact]
    public async Task EditStorage_InvalidStorageName_ThrowsStorageNotFound()
    {
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(120)
            }
        };
        var command = new EditStorageCommand(Faker.Lorem.Letter(100), model);
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await Mediator.Send(command));
    }

    private Storage GetStorage() 
        => GetContext<StorageTestContext>()
            .Storages
            .First(x => x.Type == StorageType.Warehouse);
}