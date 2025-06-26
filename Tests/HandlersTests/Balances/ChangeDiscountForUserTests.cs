using Core.Redis;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Balances.ChangeDiscountOfUser;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class ChangeDiscountForUserTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private AspNetUser _mockUser = null!;
    
    public ChangeDiscountForUserTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
        
    public async Task InitializeAsync()
    {
        _mockUser = await _context.AddMockUser();
        await _context.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task ChangeUsersDiscount_WithNegativeDiscount_FailsValidation()
    {
        var command = new ChangeDiscountForUserCommand(_mockUser.Id, -1);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task ChangeUsersDiscount_WithTooLargeDiscount_FailsValidation()
    {
        var command = new ChangeDiscountForUserCommand(_mockUser.Id, 101);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task ChangeUsersDiscount_WithEmptyUserId_FailsValidation()
    {
        var command = new ChangeDiscountForUserCommand(" ", 50);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task ChangeUsersDiscount_WithNormalDiscount_Succeeds()
    {
        var command = new ChangeDiscountForUserCommand(_mockUser.Id, 20);
        var result = await _mediator.Send(command);
        Assert.Equal(Unit.Value, result);
        
        var userDiscount = await _context.UserDiscounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _mockUser.Id);
        Assert.NotNull(userDiscount);
        Assert.Equal(20, userDiscount.Discount);
        var redis = Redis.GetRedis();
        var redisValue = redis.StringGet($"userDiscount:{_mockUser.Id}");
        Assert.Equal(20, redisValue);
    }
}