using Domain;

namespace Main.Entities.Auth;

public class UserRole : Entity<UserRole, (Guid, string)>
{
    public Guid UserId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime AssignedAt { get; set; }

    public override (Guid, string) GetId() => (UserId, RoleName);
}