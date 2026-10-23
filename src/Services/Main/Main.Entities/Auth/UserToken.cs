using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Enums;

namespace Main.Entities.Auth;

public class UserToken : AuditableEntity<UserToken, Guid>, ILinqEntity<UserToken, Guid>
{
	private static IReadOnlyDictionary<TokenType, TimeSpan> _tokenLifetime =
		new Dictionary<TokenType, TimeSpan>
		{
			[TokenType.RefreshToken] = TimeSpan.FromDays(30)
		};

	public Guid Id { get; private set; }
	public Guid UserId { get; private set; }
	public string TokenHash { get; private set; } = null!;
	public TokenType Type { get; private set; }
	public DateTime? ExpiresAt { get; private set; }
	public bool Revoked { get; private set; }
	public string? RevokeReason { get; private set; }
	public uint RowVersion { get; private set; }

	public bool IsActive => ExpiresAt < DateTime.UtcNow && !Revoked;
	public User.User User { get; private set; } = null!;

	private UserToken(
		Guid userId,
		string token,
		TokenType type,
		DateTime? expiresAt)
	{
		Id = Guid.NewGuid();
		UserId = userId;
		Type = type;
		ExpiresAt = expiresAt;
		RotateToken(token);
	}

	public static UserToken CreateRefreshToken(Guid userId, string token)
		=> new(userId, token, TokenType.RefreshToken, DateTime.UtcNow.Add(_tokenLifetime[TokenType.RefreshToken]));

	public void RotateToken(string token)
	{
		if (Revoked)
			throw new InvalidOperationException("Can not rotate revoked token.");
		if (!_tokenLifetime.TryGetValue(Type, out var lifetime))
			throw new InvalidOperationException($"Token {Type} doesn't support rotation.");

		TokenHash = token.EnsureNotNullOrWhiteSpace(
			() => new InvalidOperationException("Token hash can not  be empty"));
		ExpiresAt = DateTime.UtcNow.Add(lifetime);
	}


	public static Expression<Func<UserToken, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<UserToken, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public override Guid GetId() => Id;
}
