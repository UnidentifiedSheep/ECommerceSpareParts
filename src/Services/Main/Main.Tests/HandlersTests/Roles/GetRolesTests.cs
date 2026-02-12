using Abstractions.Models;
using Extensions;
using Main.Application.Handlers.Roles.GetRoles;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Roles;

[Collection("Combined collection")]
public class GetRolesTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    public GetRolesTests(CombinedContainerFixture fixture)
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

    [Fact]
    public async Task GetRoles_EmptyDb_ReturnsEmpty()
    {
        var query = new GetRolesQuery(null, new PaginationModel(0, 10));
        var result = await _mediator.Send(query);
        Assert.NotNull(result);
        Assert.Empty(result.Roles);
    }

    [Fact]
    public async Task GetRoles_WithRoles_ReturnsMatchingRoles()
    {
        var roles = new[] { "Admin", "User", "Manager" };
        foreach (var r in roles)
        {
            await _context.Roles.AddAsync(new Role
            {
                Id = Guid.NewGuid(),
                Name = r.ToNormalized(),
                NormalizedName = r.ToNormalized(),
                Description = r + " desc",
                IsSystem = false
            });
        }
        await _context.SaveChangesAsync();

        var queryAll = new GetRolesQuery(null, new PaginationModel(0, 10));
        var resultAll = await _mediator.Send(queryAll);
        Assert.Equal(3, resultAll.Roles.Count());

        var querySearch = new GetRolesQuery("adm", new PaginationModel(0, 10));
        var resultSearch = await _mediator.Send(querySearch);
        Assert.Single(resultSearch.Roles);
    }
}
