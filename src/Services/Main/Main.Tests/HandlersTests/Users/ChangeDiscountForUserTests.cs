using Core.Interfaces.CacheRepositories;
using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.Users.ChangeUserDiscount;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Users;

[Collection("Combined collection")]
public class ChangeDiscountForUserTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly IRedisUserRepository _redisUserRepository;
    private User _mockUser = null!;

    public ChangeDiscountForUserTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
        _redisUserRepository = sp.GetRequiredService<IRedisUserRepository>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        _mockUser = await _context.Users.AsNoTracking().FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task ChangeUsersDiscount_WithNegativeDiscount_FailsValidation()
    {
        var command = new ChangeUserDiscountCommand(_mockUser.Id, -1);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task ChangeUsersDiscount_WithTooLargeDiscount_FailsValidation()
    {
        var command = new ChangeUserDiscountCommand(_mockUser.Id, 101);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task ChangeUsersDiscount_WithNormalDiscount_Succeeds()
    {
        var command = new ChangeUserDiscountCommand(_mockUser.Id, 20);
        var result = await _mediator.Send(command);
        Assert.Equal(Unit.Value, result);

        var userDiscount = await _context.UserDiscounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _mockUser.Id);
        Assert.NotNull(userDiscount);
        Assert.Equal(20, userDiscount.Discount);
        var redisValue = await _redisUserRepository.GetUserDiscount(_mockUser.Id);
        Assert.Equal(20, redisValue);
    }
}