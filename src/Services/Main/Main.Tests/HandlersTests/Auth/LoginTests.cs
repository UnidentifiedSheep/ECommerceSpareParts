using Abstractions.Interfaces.Validators;
using FluentAssertions;
using Main.Application.Handlers.Auth;
using Main.Entities.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Auth;

public class LoginTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private const string Password = "valid-password";

	private const string UserName = "login-user";

	private const string PrimaryEmail = "login-primary@example.com";

	private const string SecondaryEmail = "login-secondary@example.com";

	[Theory]
	[InlineData(UserName)]
	[InlineData(PrimaryEmail)]
	public async Task Login_WithUserNameOrPrimaryEmail_ReturnsTokens(string login)
	{
		await CreateUser();

		var result = await Mediator.Send(
			new LoginCommand(
				login,
				Password,
				null,
				null));

		result.Token.Should().NotBeNullOrWhiteSpace();
		result.RefreshToken.Should().NotBeNullOrWhiteSpace();
		result.DeviceId.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task Login_WithSecondaryEmail_ThrowsWrongCredentialsException()
	{
		await CreateUser();

		var action = () => Mediator.Send(
			new LoginCommand(
				SecondaryEmail,
				Password,
				null,
				null));

		await action.Should().ThrowAsync<WrongCredentialsException>();
	}

	private async Task CreateUser()
	{
		var passwordManager = Scope.ServiceProvider.GetRequiredService<IPasswordManager>();

		await new MemberUserBuilder(Faker)
			.WithUserName(UserName)
			.WithPasswordHash(passwordManager.GetHashOfPassword(Password))
			.WithEmail(PrimaryEmail, isPrimary: true)
			.WithEmail(SecondaryEmail)
			.BuildAndAddToDb(Context);

		Context.ChangeTracker.Clear();
	}
}
