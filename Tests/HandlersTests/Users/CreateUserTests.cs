using Application.Handlers.Users.CreateUser;
using Core.Extensions;
using Exceptions.Exceptions.Users;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Users;

[Collection("Combined collection")]
public class CreateUserTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    public CreateUserTests(CombinedContainerFixture fixture)
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
    public async Task CreateUser_WithInvalidUsername_ThrowsValidationException()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        //Short username
        user.UserName = "s";
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));

        //Empty username
        user.UserName = "                ";
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithInvalidPhoneNumber_ThrowsValidationException()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        user.PhoneNumber = Global.Faker.Phone.PhoneNumber()[..5];
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ThrowsValidationException()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        user.PhoneNumber = Global.Faker.Person.Email[..5];
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithSameUserName_ThrowsUserNameAlreadyTakenException()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        await _mediator.Send(command);
        await Assert.ThrowsAsync<UserNameAlreadyTakenException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithSameEmail_ThrowsEmailAlreadyTakenException()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        await _mediator.Send(command);
        user.UserName = "test";
        await Assert.ThrowsAsync<EmailAlreadyTakenException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithValidData_Succeeds()
    {
        var user = MockData.MockData.GetUserDto();
        var command = new CreateUserCommand(user);

        var result = await _mediator.Send(command);
        var userInDb = await _context.AspNetUsers.FirstOrDefaultAsync(x => x.Id == result.UserId);

        Assert.NotNull(userInDb);
        Assert.Equal(user.UserName, userInDb.UserName);
        Assert.Equal(user.PhoneNumber!.ToNormalizedPhoneNumber(), userInDb.PhoneNumber);
        Assert.Equal(user.Email, userInDb.Email);
    }
}