using FluentAssertions;
using Main.Entities.Auth;
using Main.Enums;

namespace Tests.Domain.Auth;

public sealed class UserTokenTests
{
	[Fact]
	public void CreateRefreshToken_CreatesActiveTokenForThirtyDays()
	{
		var userId = Guid.NewGuid();
		var before = DateTime.UtcNow;

		var token = UserToken.CreateRefreshToken(userId, "hashed-token");

		token.Id.Should().NotBeEmpty();
		token.UserId.Should().Be(userId);
		token.TokenHash.Should().Be("hashed-token");
		token.Type.Should().Be(TokenType.RefreshToken);
		token.Revoked.Should().BeFalse();
		token.RevokeReason.Should().BeNull();
		token.ExpiresAt.Should().BeOnOrAfter(before.AddDays(30)).And.BeOnOrBefore(DateTime.UtcNow.AddDays(30));
		token.IsActive.Should().BeTrue();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" \t ")]
	public void CreateRefreshToken_RejectsEmptyHash(string? hash)
	{
		var action = () => UserToken.CreateRefreshToken(Guid.NewGuid(), hash!);

		action.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void RotateToken_ReplacesHashAndPreservesIdentity()
	{
		var token = UserToken.CreateRefreshToken(Guid.NewGuid(), "first-hash");
		var id = token.Id;
		var userId = token.UserId;
		var before = DateTime.UtcNow;

		token.RotateToken("second-hash");

		token.Id.Should().Be(id);
		token.UserId.Should().Be(userId);
		token.TokenHash.Should().Be("second-hash");
		token.ExpiresAt.Should().BeOnOrAfter(before.AddDays(30)).And.BeOnOrBefore(DateTime.UtcNow.AddDays(30));
		token.IsActive.Should().BeTrue();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" \t ")]
	public void RotateToken_InvalidHashDoesNotChangeToken(string? hash)
	{
		var token = UserToken.CreateRefreshToken(Guid.NewGuid(), "original-hash");
		var expiresAt = token.ExpiresAt;

		var action = () => token.RotateToken(hash!);

		action.Should().Throw<InvalidOperationException>();
		token.TokenHash.Should().Be("original-hash");
		token.ExpiresAt.Should().Be(expiresAt);
	}
}
