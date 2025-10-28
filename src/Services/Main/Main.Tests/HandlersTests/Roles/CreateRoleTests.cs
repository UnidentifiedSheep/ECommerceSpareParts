using Bogus;
using Core.Extensions;
using Exceptions.Exceptions.Roles;
using Main.Application.Handlers.Roles.CreateRole;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Roles;

[Collection("Combined collection")]
public class CreateRoleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly IMediator _mediator;

    public CreateRoleTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public async Task CreateRole_InvalidName_ThrowsValidation(string invalidName)
    {
        var command = new CreateRoleCommand(invalidName, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ThrowsRoleAlreadyExistsException()
    {
        var name = _faker.Random.String2(5, 12);
        var command = new CreateRoleCommand(name, "desc");
        await _mediator.Send(command);

        var dup = new CreateRoleCommand(name, "another");
        await Assert.ThrowsAsync<RoleAlreadyExistsException>(async () => await _mediator.Send(dup));
    }

    [Fact]
    public async Task CreateRole_ValidData_Succeeds()
    {
        var name = _faker.Random.String2(5, 12);
        var description = _faker.Lorem.Sentence(6);
        var command = new CreateRoleCommand(name, description);

        await _mediator.Send(command);

        var roleInDb = await _context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedName == name.ToNormalized());
        
        Assert.NotNull(roleInDb);
        Assert.Equal(name.ToNormalized(), roleInDb.NormalizedName);
        Assert.Equal(description, roleInDb.Description);
        Assert.False(roleInDb.IsSystem);
    }
}
