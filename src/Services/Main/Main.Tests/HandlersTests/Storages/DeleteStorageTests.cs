using Main.Application.Handlers.Storages.DeleteStorage;
using Main.Entities.Exceptions.Storages;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Storages;

public class DeleteStorageTests : IntegrationTest
{
    public DeleteStorageTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
    }

    [Fact]
    public async Task DeleteStorage_EmptyName_ThrowsValidationException()
    {
        var command = new DeleteStorageCommand("");
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteStorage_NotExisting_ThrowsStorageNotFoundException()
    {
        var command = new DeleteStorageCommand("some-non-existing-storage");
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteStorage_WithNoContents_Succeeds()
    {
        var storage = GetContext<StorageTestContext>()
            .Storages
            .First(x => x.Name != GetContext<StorageContentTestContext>().StorageContents.First().StorageName);
        var command = new DeleteStorageCommand(storage.Name);
        await Mediator.Send(command);

        var exists = await Context.Storages.AnyAsync(x => x.Name == storage.Name);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteStorage_WithContents_ThrowsDbUpdateException()
    {
        var storage = GetContext<StorageContentTestContext>().StorageContents.First().StorageName;

        var command = new DeleteStorageCommand(storage);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await Mediator.Send(command));

        var exists = await Context.Storages.AnyAsync(x => x.Name == storage);
        Assert.True(exists);
    }
}