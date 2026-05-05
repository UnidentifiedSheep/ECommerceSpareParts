using Main.Abstractions.Constants;
using Main.Application.Handlers.Storages.CreateStorage;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Storage;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Storages;

public class CreateStorageTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task CreateStorage_TooLargeName_FailsValidation()
    {
        var command = GetCommand() with{ Name = Faker.Lorem.Letter(500) };
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooSmallName_FailsValidation()
    {
        var command = GetCommand() with { Name = Faker.Lorem.Letter() };
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooLargeDescription_FailsValidation()
    {
        var command = GetCommand() with { Description = Faker.Lorem.Letter(600) };
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_ExistingName_ThrowStorageNameIsTaken()
    {
        var storageModel = await new StorageBuilder(Faker)
            .BuildAndAddToDb(Context);
        
        var command = new CreateStorageCommand(
                storageModel.Name, 
                storageModel.Description, 
                storageModel.Location, 
                storageModel.Type);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.StoragesNameAlreadyTaken, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateStorage_Normal_Succeeds()
    {
        var command = GetCommand();
        await Mediator.Send(command);

        var createdStorage = await Context.Storages.FirstOrDefaultAsync(x => x.Name == command.Name);
        Assert.NotNull(createdStorage);

        Assert.Equal(command.Description, createdStorage.Description);
        Assert.Equal(command.Location, createdStorage.Location);
        Assert.Equal(command.Type, createdStorage.Type);
    }

    private CreateStorageCommand GetCommand()
    {
        var storageModel = new StorageBuilder(Faker).Build();
        return new CreateStorageCommand(
            storageModel.Name, 
            storageModel.Description, 
            storageModel.Location, 
            storageModel.Type);
    }
}