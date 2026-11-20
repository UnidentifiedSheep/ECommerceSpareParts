using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Application.Commands.Auth;
using Main.Modules.Identity.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Auth;

public sealed class InternalServiceLoginTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private const string Service = "internal-test";
	private const string Secret = "internal-service-test-secret";

	[Fact]
	public async Task Login_ServiceReceivesLongerLivedJwtWithoutRefreshTokenOrLoginEvent()
	{
		var user = await new UserBuilder(Faker).WithUserName(Service)
			.WithUserInfo()
			.WithRole("System")
			.WithPasswordHash(
				Scope.ServiceProvider.GetRequiredService<IPasswordManager>().GetHashOfPassword(Secret))
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		var before = DateTime.UtcNow;

		var result = await Mediator.Send(new InternalServiceLoginCommand(Service, Secret), CancellationToken);

		var principal = Scope.ServiceProvider
			.GetRequiredService<IJwtGenerator>()
			.GetClaimsPrincipal(result.Token);
		principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(user.Id.ToString());
		principal.IsInRole("SYSTEM").Should().BeTrue();
		new JwtSecurityTokenHandler().ReadJwtToken(result.Token)
			.ValidTo
			.Should()
			.BeOnOrAfter(before.AddMinutes(75).AddSeconds(-1))
			.And
			.BeOnOrBefore(DateTime.UtcNow.AddMinutes(75));
		(await Context.Set<UserToken>().CountAsync(CancellationToken)).Should().Be(0);
		(await Context.Set<User>().AsNoTracking().SingleAsync(x => x.Id == user.Id, CancellationToken))
			.LastLoginAt
			.Should()
			.BeNull();
	}

	[Theory]
	[InlineData(Service, "wrong-secret")]
	[InlineData("unknown-service", Secret)]
	public async Task Login_InvalidServiceCredentialsAreRejected(string service, string secret)
	{
		await new UserBuilder(Faker).WithUserName(Service)
			.WithUserInfo()
			.WithRole("System")
			.WithPasswordHash(
				Scope.ServiceProvider.GetRequiredService<IPasswordManager>().GetHashOfPassword(Secret))
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var action = () => Mediator.Send(new InternalServiceLoginCommand(service, secret), CancellationToken);

		await action.Should().ThrowAsync<WrongCredentialsException>();
		(await Context.Set<UserToken>().CountAsync(CancellationToken)).Should().Be(0);
	}
}
