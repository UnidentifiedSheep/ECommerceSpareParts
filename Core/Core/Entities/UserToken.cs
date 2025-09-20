using System.Net;

namespace Core.Entities;

public partial class UserToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public List<string> Permissions { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public string? RevokeReason { get; set; }

    public string? DeviceId { get; set; }

    public IPAddress? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
