using Abstractions.Interfaces.Services;
using FluentAssertions;
using Main.Application.Handlers.Auth.EmailVerification;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Models.Auth;
using Main.Entities.Exceptions;
using Main.Enums.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class VerifyEmailTests : IntegrationTest
{
    private const string Email = "verification@example.com";

    public VerifyEmailTests(
        CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UserContextTestContext>();
    }

    [Fact]
    public async Task VerifyEmail_ValidToken_ConfirmsEmail()
    {
        var user = await CreateUser();
        var (_, token) = await CreateToken(
            user.Id,
            VerificationType.EmailVerification,
            Email);

        await Mediator.Send(new VerifyEmailCommand(token));

        var email = await Context.UserEmails
            .AsNoTracking()
            .SingleAsync(x => x.Email == Email);
        email.Confirmed.Should().BeTrue();
        email.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyEmail_TokenIsUsedTwice_SecondAttemptThrows()
    {
        var user = await CreateUser();
        var (_, token) = await CreateToken(
            user.Id,
            VerificationType.EmailVerification,
            Email);

        await Mediator.Send(new VerifyEmailCommand(token));
        var action = () => Mediator.Send(new VerifyEmailCommand(token));

        await action.Should()
            .ThrowAsync<EmailVerificationTokenExpiredException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("malformed-token")]
    [InlineData("payload.invalid-signature")]
    public async Task VerifyEmail_MalformedToken_Throws(string token)
    {
        var action = () => Mediator.Send(new VerifyEmailCommand(token));

        await action.Should()
            .ThrowAsync<EmailVerificationTokenExpiredException>();
    }

    [Fact]
    public async Task VerifyEmail_ExpiredToken_Throws()
    {
        var user = await CreateUser();
        var signer = Scope.ServiceProvider.GetRequiredService<IJsonSigner>();
        var token = signer.Sign(
            new VerificationPayload
            {
                UserId = user.Id,
                Type = VerificationType.EmailVerification,
                DataToVerify = Email
            });

        var action = () => Mediator.Send(new VerifyEmailCommand(token));

        await action.Should()
            .ThrowAsync<EmailVerificationTokenExpiredException>();
    }

    [Fact]
    public async Task VerifyEmail_WrongVerificationType_DoesNotConsumeToken()
    {
        var user = await CreateUser();
        var (payload, token) = await CreateToken(
            user.Id,
            VerificationType.PhoneVerification,
            Email);

        var action = () => Mediator.Send(new VerifyEmailCommand(token));

        await action.Should()
            .ThrowAsync<EmailVerificationTokenExpiredException>();
        (await GetPayloadProvider().TryConsumeToken(payload.Id))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task VerifyEmail_EmailBelongsToAnotherUser_DoesNotConsumeToken()
    {
        await CreateUser();
        var anotherUser = await CreateUser(email: null);
        var (payload, token) = await CreateToken(
            anotherUser.Id,
            VerificationType.EmailVerification,
            Email);

        var action = () => Mediator.Send(new VerifyEmailCommand(token));

        await action.Should()
            .ThrowAsync<UserEmailNotFoundException>();
        (await GetPayloadProvider().TryConsumeToken(payload.Id))
            .Should()
            .BeTrue();
    }

    private async Task<Main.Entities.User.User> CreateUser(
        string? email = Email)
    {
        var builder = new MemberUserBuilder(Faker);
        if (email is not null)
            builder.WithEmail(
                email,
                isConfirmed: false);

        var user = await builder.BuildAndAddToDb(Context);
        Context.ChangeTracker.Clear();
        return user;
    }

    private async Task<(VerificationPayload Payload, string Token)> CreateToken(
        Guid userId,
        VerificationType type,
        string email)
    {
        var payload = await GetPayloadProvider()
            .GetPayload(userId, type, email);
        var signer = Scope.ServiceProvider.GetRequiredService<IJsonSigner>();
        return (payload, signer.Sign(payload));
    }

    private IVerificationPayloadProvider GetPayloadProvider()
    {
        return Scope.ServiceProvider
            .GetRequiredService<IVerificationPayloadProvider>();
    }
}
