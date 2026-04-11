using System.Net;
using Domain;
using Main.Enums;

namespace Main.Entities;

public class UserToken : AuditableEntity<UserToken, Guid>
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

    public virtual User User { get; set; } = null!;
    public override Guid GetId() => Id;
}