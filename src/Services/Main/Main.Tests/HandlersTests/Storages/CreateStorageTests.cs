using Bogus;
using Main.Abstractions.Constants;
using Main.Application.Configs;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Storages;

[Collection("Combined collection")]
public class CreateStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Locale);
    private readonly IMediator _mediator;

    public CreateStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateStorage_TooLargeName_FailsValidation()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(_faker.Lorem.Letter(200), storage.Description, storage.Location, StorageType.Warehouse);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooSmallName_FailsValidation()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(_faker.Lorem.Letter(), storage.Description, storage.Location, StorageType.Warehouse);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooLargeDescription_FailsValidation()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(storage.Name, storage.Description, _faker.Lorem.Letter(500), StorageType.Warehouse);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_ExistingName_ThrowStorageNameIsTaken()
    {
        var storage = CreateNewStorage(1)[0];
        storage.Type = StorageType.Warehouse;
        await _context.Storages.AddAsync(storage);
        await _context.SaveChangesAsync();
        var command = new CreateStorageCommand(storage.Name, storage.Description, storage.Location, StorageType.Warehouse);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.StoragesNameAlreadyTaken, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateStorage_Normal_Succeeds()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(storage.Name, storage.Description, storage.Location, StorageType.Warehouse);
        await _mediator.Send(command);
        
        var createdStorage = await _context.Storages.FirstOrDefaultAsync(x => x.Name == storage.Name);
        Assert.NotNull(createdStorage);
        
        Assert.Equal(storage.Description, createdStorage.Description);
        Assert.Equal(storage.Location, createdStorage.Location);
        Assert.Equal(StorageType.Warehouse, createdStorage.Type);
    }
}