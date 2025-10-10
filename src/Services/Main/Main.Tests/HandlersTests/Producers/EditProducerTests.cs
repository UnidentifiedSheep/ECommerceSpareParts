using Bogus;
using Core.Models;
using Main.Application.Handlers.Producers.EditProducer;
using Main.Core.Dtos.Amw.Producers;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Producers;

[Collection("Combined collection")]
public class EditProducerTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Global.Locale);
    private readonly IMediator _mediator;

    public EditProducerTests(CombinedContainerFixture fixture)
    {
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
    public async Task EditProducer_TooLargeName_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(200)
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_TooSmallName_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter()
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_IsSetButNullValue_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = null
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_DescriptionTooLarge_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(1000)
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_NoValuesSet_Succeeds()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto();
        var command = new EditProducerCommand(producer.Id, model);
        await _mediator.Send(command);

        var afterEditing = await _context.Producers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == producer.Id);
        Assert.NotNull(afterEditing);
        Assert.Equal(producer.Name, afterEditing.Name);
        Assert.Equal(producer.Description, afterEditing.Description);
        Assert.Equal(producer.IsOe, afterEditing.IsOe);
    }

    [Fact]
    public async Task EditProducer_Normal_Succeeds()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(30)
            },
            Description =
            {
                IsSet = true,
                Value = _faker.Lorem.Letter(30)
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        await _mediator.Send(command);

        var afterEditing = await _context.Producers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == producer.Id);
        Assert.NotNull(afterEditing);
        Assert.Equal(model.Name, afterEditing.Name);
        Assert.Equal(model.Description, afterEditing.Description);
        Assert.Equal(producer.IsOe, afterEditing.IsOe);
    }
}