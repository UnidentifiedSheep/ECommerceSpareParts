using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Main.Application.Dtos.Users;
using Main.Application.Models;
using Main.Application.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Tests.Services;

public sealed class JwtGeneratorTests
{
	private static JwtOptions Configuration => new()
	{
		ValidDuration = TimeSpan.FromMinutes(15),
		ValidIssuer = "auth-tests",
		IssuerSigningKey = "auth-tests-signing-key-with-at-least-32-characters"
	};

	[Theory]
	[InlineData(0)]
	[InlineData(60)]
	public void CreateToken_PreservesIdentityRolesPermissionsAndLifetime(int additionalMinutes)
	{
		var generator = new JwtGenerator(Options.Create(Configuration));
		var user = CreateUser();
		var before = DateTime.UtcNow;

		var token = generator.CreateToken(user, ["Member", "Manager"], ["PRODUCTS_GET", "ORDERS_GET"], TimeSpan.FromMinutes(additionalMinutes));

		var principal = generator.GetClaimsPrincipal(token);
		principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(user.Id.ToString());
		principal.FindFirstValue(ClaimTypes.Name).Should().Be(user.UserName);
		principal.FindFirstValue(ClaimTypes.GivenName).Should().Be("Test User");
		principal.FindAll(ClaimTypes.Role).Select(x => x.Value).Should().BeEquivalentTo(["Member", "Manager"]);
		principal.FindAll("permission").Select(x => x.Value).Should().BeEquivalentTo(["PRODUCTS_GET", "ORDERS_GET"]);
		var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
		jwt.Issuer.Should().Be(Configuration.ValidIssuer);
		jwt.ValidTo.Should().BeOnOrAfter(before.AddMinutes(15 + additionalMinutes).AddSeconds(-1))
			.And.BeOnOrBefore(DateTime.UtcNow.AddMinutes(15 + additionalMinutes));
	}

	[Fact]
	public void GetClaimsPrincipal_RejectsTokenSignedWithAnotherKey()
	{
		var generator = new JwtGenerator(Options.Create(Configuration));
		var another = new JwtGenerator(Options.Create(Configuration with
		{
			IssuerSigningKey = "another-signing-key-with-at-least-32-characters"
		}));
		var token = another.CreateToken(CreateUser(), ["Member"], []);

		var action = () => generator.GetClaimsPrincipal(token);

		action.Should().Throw<SecurityTokenException>();
	}

	[Fact]
	public void GetClaimsPrincipal_RejectsExpiredToken()
	{
		var settings = Configuration;
		var generator = new JwtGenerator(Options.Create(settings));
		var handler = new JwtSecurityTokenHandler();
		var token = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
		{
			Issuer = settings.ValidIssuer,
			SigningCredentials = settings.SigningCredentials,
			NotBefore = DateTime.UtcNow.AddMinutes(-20),
			Expires = DateTime.UtcNow.AddMinutes(-10),
			Subject = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())])
		}));

		var action = () => generator.GetClaimsPrincipal(token);

		action.Should().Throw<SecurityTokenExpiredException>();
	}

	[Fact]
	public void CreateRefreshToken_ReturnsDistinctBase64EncodedSecrets()
	{
		var generator = new JwtGenerator(Options.Create(Configuration));

		var first = generator.CreateRefreshToken();
		var second = generator.CreateRefreshToken();

		first.Should().NotBe(second);
		Convert.FromBase64String(first).Should().HaveCount(64);
		Convert.FromBase64String(second).Should().HaveCount(64);
	}

	private static UserDto CreateUser() => new()
	{
		Id = Guid.NewGuid(), UserName = "test-user", NormalizedUserName = "TEST-USER",
		CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
		TwoFactorEnabled = false, LockoutEnd = null, AccessFailedCount = 0, LastLoginAt = null,
		UserInfo = new UserInfoDto { Name = "Test", Surname = "User" }
	};
}
