using Security.Core.Interfaces;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Main.Application.Handlers.Auth;
using Main.Application.Interfaces.Services;
using Main.Application.Notifications;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
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
				null),
			CancellationToken);

		result.Token.Should().NotBeNullOrWhiteSpace();
		result.RefreshToken.Should().NotBeNullOrWhiteSpace();
		var stored = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		stored.TokenHash.Should().Be(Scope.ServiceProvider.GetRequiredService<IValueHasher>().Hash(result.RefreshToken));
		stored.TokenHash.Should().NotBe(result.RefreshToken);
		stored.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(10));
		stored.IsActive.Should().BeTrue();
		var principal = Scope.ServiceProvider.GetRequiredService<IJwtGenerator>().GetClaimsPrincipal(result.Token);
		principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(stored.UserId.ToString());
		principal.FindFirstValue(ClaimTypes.Name).Should().Be(UserName);
		principal.IsInRole("MEMBER").Should().BeTrue();
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
		(await Context.UserTokens.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Theory]
	[InlineData(UserName, "wrong-password")]
	[InlineData("unknown-user", Password)]
	public async Task Login_WrongCredentialsDoNotCreateTokensOrRevealPassword(string login, string password)
	{
		await CreateUser();

		var action = () => Mediator.Send(new LoginCommand(login, password, null, null), CancellationToken);

		var exception = (await action.Should().ThrowAsync<WrongCredentialsException>()).Which;
		exception.GetErrorValues().Should().BeEquivalentTo(new { Login = (string?)null, Password = (string?)null });
		(await Context.UserTokens.CountAsync(CancellationToken)).Should().Be(0);
		(await Context.Users.AsNoTracking().SingleAsync(x => x.UserName.NormalizedValue == "LOGIN-USER", CancellationToken))
			.LastLoginAt.Should().BeNull();
	}

	[Fact]
	public async Task Login_SystemUserCannotUsePasswordLogin()
	{
		var passwordManager = Scope.ServiceProvider.GetRequiredService<IPasswordManager>();
		await new UserBuilder(Faker).WithUserName("system-login-user")
			.WithPasswordHash(passwordManager.GetHashOfPassword(Password))
			.WithRole("System").WithUserInfo().BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var action = () => Mediator.Send(new LoginCommand("system-login-user", Password, null, null), CancellationToken);

		await action.Should().ThrowAsync<WrongCredentialsException>();
		(await Context.UserTokens.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task Login_PersistsTimestampAndQueuesNotificationWithRequestContext()
	{
		await CreateUser();
		var before = DateTime.UtcNow;
		var address = IPAddress.Parse("2001:db8::15");

		await Mediator.Send(new LoginCommand(UserName, Password, address, "Mozilla/5.0 Chrome/138.0 Windows"), CancellationToken);

		var token = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		var user = await Context.Users.AsNoTracking().SingleAsync(x => x.Id == token.UserId, CancellationToken);
		user.LastLoginAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
		var notification = await Context.Notifications.AsNoTracking()
			.SingleAsync(x => x.UserId == user.Id && x.NotificationSystemName == "UserLoggedIn", CancellationToken);
		var model = JsonSerializer.Deserialize<UserLoggedInNotificationData>(notification.Model, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
		model.IpAddress.Should().Be(address.ToString());
		model.Device.Should().Be("Chrome · Windows");
	}

	[Fact]
	public async Task Login_IndependentLoginsCreateDistinctRefreshTokens()
	{
		await CreateUser();
		var first = await Mediator.Send(new LoginCommand(UserName, Password, null, null), CancellationToken);
		var second = await Mediator.Send(new LoginCommand(UserName, Password, null, null), CancellationToken);

		first.RefreshToken.Should().NotBe(second.RefreshToken);
		var hashes = await Context.UserTokens.AsNoTracking().Select(x => x.TokenHash).ToListAsync(CancellationToken);
		var hasher = Scope.ServiceProvider.GetRequiredService<IValueHasher>();
		hashes.Should().BeEquivalentTo([hasher.Hash(first.RefreshToken), hasher.Hash(second.RefreshToken)]);
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
