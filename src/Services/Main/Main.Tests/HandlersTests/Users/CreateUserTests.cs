using Bogus;
using Main.Abstractions.Consts;
using Main.Application.Handlers.Users.CreateUser;
using Main.Abstractions.Dtos.Emails;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Users;

[Collection("Combined collection")]
public class CreateUserTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Global.Locale);
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

    [Theory]
    [InlineData("s")]
    [InlineData("              ")]
    public async Task CreateUser_WithInvalidUsername_ThrowsValidationException(string invalidUsername)
    {
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var command = new CreateUserCommand(invalidUsername, _faker.Lorem.Letter(10),
            userInfo, [], [], []);
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
            _faker.Lorem.Letter(10), userInfo, [email], [], []);

        await _mediator.Send(command);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.UserNameAlreadyTaken, exception.Failures[0].ErrorName);
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
        var error = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(scommand));
        Assert.Equal(ApplicationErrors.UserEmailAlreadyTaken, error.Failures[0].ErrorName);
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