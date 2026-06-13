using System.Linq.Expressions;
using System.Net;
using Domain;
using Domain.Interfaces;
using Main.Enums;

namespace Main.Entities.Auth;

public class UserToken : AuditableEntity<UserToken, Guid>, ILinqEntity<UserToken, Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public List<string> Permissions { get; set; } = null!;

    public TokenType Type { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public string? RevokeReason { get; set; }

    public string? DeviceId { get; set; }

    public IPAddress? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public virtual User.User User { get; set; } = null!;

    public static Expression<Func<UserToken, Guid>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<UserToken, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public override Guid GetId()
    {
        return Id;
    }
}
