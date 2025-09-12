using Application.Configs;
using Application.Handlers.Storages.EditStorage;
using Bogus;
using Core.Dtos.Amw.Storage;
using Core.Entities;
using Core.Models;
using Exceptions.Exceptions.Storages;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Storages;

[Collection("Combined collection")]
public class EditStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Locale);
    private readonly IMediator _mediator;
    private Storage _storage = null!;

    public EditStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        _storage = await _context.Storages.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task EditStorage_IsNotSetButHasDescriptionValue_Succeeds()
    {
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = false,
                Value = _faker.Lorem.Sentence(30)
            }
        };
        var command = new EditStorageCommand(_storage.Name, model);
        await _mediator.Send(command);
        var storage = await _context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == _storage.Name);
        Assert.NotNull(storage);

        Assert.Equal(_storage.Name, storage.Name);
        Assert.Equal(_storage.Description, storage.Description);
        Assert.Equal(_storage.Location, storage.Location);
    }

    [Fact]
    public async Task EditStorage_IsNotSetButHasLocationValue_Succeeds()
    {
        var model = new PatchStorageDto
        {
            Location = new PatchField<string?>
            {
                IsSet = false,
                Value = _faker.Lorem.Sentence(30)
            }
        };
        var command = new EditStorageCommand(_storage.Name, model);
        await _mediator.Send(command);

        var storage = await _context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == _storage.Name);
        Assert.NotNull(storage);

        Assert.Equal(_storage.Name, storage.Name);
        Assert.Equal(_storage.Description, storage.Description);
        Assert.Equal(_storage.Location, storage.Location);
    }

    [Fact]
    public async Task EditStorage_TooLargeLocation_FailsValidation()
    {
        var model = new PatchStorageDto
        {
            Location = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(300)
            }
        };
        var command = new EditStorageCommand(_storage.Name, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditStorage_TooLargeDesctiption_FailsValidation()
    {
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(300)
            }
        };
        var command = new EditStorageCommand(_storage.Name, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditStorage_Normal_Succeeds()
    {
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(120)
            },
            Location = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(120)
            }
        };
        var command = new EditStorageCommand(_storage.Name, model);
        await _mediator.Send(command);

        var storage = await _context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == _storage.Name);
        Assert.NotNull(storage);
        Assert.Equal(storage.Description, model.Description);
        Assert.Equal(storage.Location, model.Location);
    }

    [Fact]
    public async Task EditStorage_InvalidStorageName_ThrowsStorageNotFound()
    {
        var model = new PatchStorageDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(120)
            }
        };
        var command = new EditStorageCommand(_faker.Lorem.Letter(100), model);
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await _mediator.Send(command));
        var storage = await _context.Storages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == _storage.Name);
        Assert.NotNull(storage);
        Assert.Equal(_storage.Description, storage.Description);
    }
}