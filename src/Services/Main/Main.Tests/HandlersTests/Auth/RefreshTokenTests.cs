using System.Security.Claims;
using FluentAssertions;
using Main.Application.Handlers.Auth;
using Enums;
using Role = Main.Entities.Auth.Role;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Auth;

public sealed class RefreshTokenTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private const string RawToken = "original-refresh-token";

	[Fact]
	public async Task Refresh_ValidTokenRotatesSameRowAndReturnsSignedJwt()
	{
		var stored = await CreateTokenAsync(DateTime.UtcNow.AddMinutes(5));
		var before = DateTime.UtcNow;

		var result = await RefreshAsync(RawToken);

		var saved = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		result.RefreshToken.Should().NotBeNullOrWhiteSpace().And.NotBe(RawToken);
		saved.Id.Should().Be(stored.Id);
		saved.UserId.Should().Be(stored.UserId);
		saved.TokenHash.Should().Be(Hasher.Hash(result.RefreshToken)).And.NotBe(result.RefreshToken);
		saved.RowVersion.Should().NotBe(stored.RowVersion);
		saved.ExpiresAt.Should().BeOnOrAfter(before.AddDays(30)).And.BeOnOrBefore(DateTime.UtcNow.AddDays(30));
		saved.IsActive.Should().BeTrue();
		var principal = Scope.ServiceProvider.GetRequiredService<IJwtGenerator>().GetClaimsPrincipal(result.Token);
		principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(stored.UserId.ToString());
		principal.IsInRole("MEMBER").Should().BeTrue();
	}

	[Fact]
	public async Task Refresh_ConsumedTokenCannotBeReplayedButReplacementWorks()
	{
		var stored = await CreateTokenAsync(DateTime.UtcNow.AddDays(1));
		var first = await RefreshAsync(RawToken);

		var replay = () => RefreshAsync(RawToken);
		await replay.Should().ThrowAsync<InvalidTokenException>();
		var afterReplay = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		afterReplay.TokenHash.Should().Be(Hasher.Hash(first.RefreshToken));

		var second = await RefreshAsync(first.RefreshToken);

		var saved = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		saved.Id.Should().Be(stored.Id);
		saved.TokenHash.Should().Be(Hasher.Hash(second.RefreshToken));
		second.RefreshToken.Should().NotBe(first.RefreshToken);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("unknown-token")]
	public async Task Refresh_UnknownOrMissingTokenIsRejected(string? rawToken)
	{
		var action = () => RefreshAsync(rawToken!);

		await action.Should().ThrowAsync<InvalidTokenException>();
		(await Context.UserTokens.CountAsync(CancellationToken)).Should().Be(0);
	}

	[Theory]
	[InlineData(-1, false, TokenType.RefreshToken)]
	[InlineData(1, true, TokenType.RefreshToken)]
	[InlineData(-1, true, TokenType.RefreshToken)]
	[InlineData(null, false, TokenType.RefreshToken)]
	[InlineData(1, false, TokenType.JwtToken)]
	public async Task Refresh_IneligibleTokenIsRejectedWithoutChangingIt(int? expiryDays, bool revoked, TokenType type)
	{
		var stored = await CreateTokenAsync(expiryDays is { } days ? DateTime.UtcNow.AddDays(days) : null, revoked, type);

		var action = () => RefreshAsync(RawToken);

		await action.Should().ThrowAsync<InvalidTokenException>();
		var saved = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		saved.TokenHash.Should().Be(stored.TokenHash);
		saved.ExpiresAt.Should().Be(stored.ExpiresAt);
		saved.RowVersion.Should().Be(stored.RowVersion);
		if (revoked || expiryDays is null or <= 0)
			saved.IsActive.Should().BeFalse();
	}

	[Fact]
	public async Task Refresh_ConcurrentRequestsHaveOnlyOneWinner()
	{
		await CreateTokenAsync(DateTime.UtcNow.AddDays(1));

		async Task<(RefreshTokenResult? Result, Exception? Error)> AttemptAsync()
		{
			try { return (await RefreshAsync(RawToken), null); }
			catch (Exception error) { return (null, error); }
		}
		var attempts = await Task.WhenAll(AttemptAsync(), AttemptAsync());

		var winner = attempts.Where(x => x.Result is not null).Should().ContainSingle().Which.Result!;
		attempts.Where(x => x.Error is not null).Should().ContainSingle().Which.Error.Should().BeOfType<InvalidTokenException>();
		var saved = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		saved.TokenHash.Should().Be(Hasher.Hash(winner.RefreshToken));
	}

	[Fact]
	public async Task SaveChanges_ConcurrentRotationDetectsStaleRowVersion()
	{
		var stored = await CreateTokenAsync(DateTime.UtcNow.AddDays(1));
		using var firstScope = Sp.CreateScope();
		using var secondScope = Sp.CreateScope();
		var firstContext = firstScope.ServiceProvider.GetRequiredService<DContext>();
		var secondContext = secondScope.ServiceProvider.GetRequiredService<DContext>();
		var first = await firstContext.UserTokens.SingleAsync(x => x.Id == stored.Id, CancellationToken);
		var second = await secondContext.UserTokens.SingleAsync(x => x.Id == stored.Id, CancellationToken);
		first.RotateToken(Hasher.Hash("winner"));
		second.RotateToken(Hasher.Hash("loser"));
		await firstContext.SaveChangesAsync(CancellationToken);

		var action = () => secondContext.SaveChangesAsync(CancellationToken);

		await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
		var saved = await Context.UserTokens.AsNoTracking().SingleAsync(CancellationToken);
		saved.TokenHash.Should().Be(Hasher.Hash("winner"));
	}

	[Fact]
	public async Task Refresh_UsesCurrentProfileAndPermissionsWithoutCacheInvalidation()
	{
		var stored = await CreateTokenAsync(DateTime.UtcNow.AddDays(1));
		Context.AddRange(new Permission(PermissionCodes.USERS_INFO_GET), new Permission(PermissionCodes.USERS_GET));
		var role = Role.Create("refresh-access");
		role.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		Context.Add(role);
		Context.Add(UserRole.Create(stored.UserId, role.Name));
		var direct = UserPermission.Create(stored.UserId, Permission.ToNormalizedPermission(PermissionCodes.USERS_GET));
		Context.Add(direct);
		await Context.SaveChangesAsync(CancellationToken);
		var generator = Scope.ServiceProvider.GetRequiredService<IJwtGenerator>();
		var first = await RefreshAsync(RawToken);
		var firstClaims = generator.GetClaimsPrincipal(first.Token);
		firstClaims.IsInRole(role.Name).Should().BeTrue();
		firstClaims.FindAll("permission").Select(claim => claim.Value).Should().BeEquivalentTo([
			Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET),
			Permission.ToNormalizedPermission(PermissionCodes.USERS_GET)]);

		role.RemovePermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		Context.Remove(direct);
		var user = await Context.Users.Include(user => user.UserInfo)
			.SingleAsync(user => user.Id == stored.UserId, CancellationToken);
		user.SetUserInfo("New", "Profile", null);
		await Context.SaveChangesAsync(CancellationToken);

		var second = await RefreshAsync(first.RefreshToken);
		var secondClaims = generator.GetClaimsPrincipal(second.Token);
		secondClaims.FindAll("permission").Should().BeEmpty();
		secondClaims.FindFirstValue(ClaimTypes.GivenName).Should().Be("New Profile");
	}

	private IValueHasher Hasher => Scope.ServiceProvider.GetRequiredService<IValueHasher>();

	private async Task<RefreshTokenResult> RefreshAsync(string token)
	{
		using var scope = Sp.CreateScope();
		return await scope.ServiceProvider.GetRequiredService<IMediator>()
			.Send(new RefreshTokenCommand(token), CancellationToken);
	}

	private async Task<UserToken> CreateTokenAsync(DateTime? expiresAt, bool revoked = false, TokenType type = TokenType.RefreshToken)
	{
		var user = await new MemberUserBuilder(Faker).BuildAndAddToDb(Context);
		var token = UserToken.CreateRefreshToken(user.Id, Hasher.Hash(RawToken));
		await Context.UserTokens.AddAsync(token, CancellationToken);
		Context.Entry(token).Property(x => x.ExpiresAt).CurrentValue = expiresAt;
		Context.Entry(token).Property(x => x.Revoked).CurrentValue = revoked;
		Context.Entry(token).Property(x => x.Type).CurrentValue = type;
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();
		return await Context.UserTokens.AsNoTracking().SingleAsync(x => x.Id == token.Id, CancellationToken);
	}
}
