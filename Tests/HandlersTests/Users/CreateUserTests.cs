using Application.Handlers.Users.CreateUser;
using Bogus;
using Core.Dtos.Emails;
using Core.Enums;
using Exceptions.Exceptions.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Users;

[Collection("Combined collection")]
public class CreateUserTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly Faker _faker = new(Global.Locale);

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

    [Theory]
    [InlineData("s")]
    [InlineData("              ")]
    public async Task CreateUser_WithInvalidUsername_ThrowsValidationException(string invalidUsername)
    {
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(invalidUsername, _faker.Lorem.Letter(10), 
            userInfo,[], [], []);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    /*[Fact]
    public async Task CreateUser_WithInvalidPhoneNumber_ThrowsValidationException()
    {
        var command = new CreateUserCommand(_faker.Lorem.Letter(10), _faker.Lorem.Letter(10), [], [(_faker.Phone.PhoneNumber()[..5];], []);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }*/

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ThrowsValidationException()
    {
        var email = new EmailDto
        {
            Email = Global.Faker.Person.Email[..5],
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Personal
        };
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(_faker.Person.UserName, _faker.Lorem.Letter(10), 
            userInfo, [email], [], []);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithSameUserName_ThrowsUserNameAlreadyTakenException()
    {
        var email = new EmailDto
        {
            Email = Global.Faker.Person.Email,
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Personal
        };
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(_faker.Lorem.Letter(10), 
            _faker.Lorem.Letter(10), userInfo,[email], [], []);

        await _mediator.Send(command);
        await Assert.ThrowsAsync<UserNameAlreadyTakenException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateUser_WithSameEmail_ThrowsEmailAlreadyTakenException()
    {
        var email = new EmailDto
        {
            Email = Global.Faker.Person.Email,
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Personal
        };
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(_faker.Person.UserName, _faker.Lorem.Letter(10), 
            userInfo, [email], [], []);
        
        await _mediator.Send(command);
        var scommand = new CreateUserCommand("sdfsdf", _faker.Lorem.Letter(10), userInfo, 
            [email], [], []);
        await Assert.ThrowsAsync<EmailAlreadyTakenException>(async () => await _mediator.Send(scommand));
    }

    [Fact]
    public async Task CreateUser_WithValidData_Succeeds()
    {
        var email = new EmailDto
        {
            Email = Global.Faker.Person.Email,
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Personal
        };
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(_faker.Person.UserName, _faker.Lorem.Letter(10), 
            userInfo, [email], [], []);

        var result = await _mediator.Send(command);
        var userInDb = await _context.Users.FirstOrDefaultAsync(x => x.Id == result.UserId);
        var emailInDb = await _context.UserEmails.FirstOrDefaultAsync(x => x.UserId == result.UserId);

        Assert.NotNull(userInDb);
        Assert.NotNull(emailInDb);
        Assert.Equal(command.UserName, userInDb.UserName);
        Assert.Equal(command.Emails.First().Email, emailInDb.Email);
    }
}