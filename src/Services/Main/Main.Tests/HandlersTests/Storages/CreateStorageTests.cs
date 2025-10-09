using Bogus;
using Exceptions.Exceptions.Storages;
using Main.Application.Configs;
using Main.Application.Handlers.Storages.CreateStorage;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;
using ValidationException = FluentValidation.ValidationException;

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
        var command = new CreateStorageCommand(_faker.Lorem.Letter(200), storage.Description, storage.Location);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooSmallName_FailsValidation()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(_faker.Lorem.Letter(), storage.Description, storage.Location);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_TooLargeDescription_FailsValidation()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(storage.Name, storage.Description, _faker.Lorem.Letter(500));
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_ExistingName_ThrowStorageNameIsTaken()
    {
        var storage = CreateNewStorage(1)[0];
        await _context.Storages.AddAsync(storage);
        await _context.SaveChangesAsync();
        var command = new CreateStorageCommand(storage.Name, storage.Description, storage.Location);
        await Assert.ThrowsAsync<StorageNameIsTakenException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateStorage_Normal_Succeeds()
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(storage.Name, storage.Description, storage.Location);
        await _mediator.Send(command);
    }
}