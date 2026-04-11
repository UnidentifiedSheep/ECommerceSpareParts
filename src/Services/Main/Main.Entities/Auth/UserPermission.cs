using Domain;

namespace Main.Entities;

public class UserPermission : AuditableEntity<UserPermission, (Guid, string)>
{
    public Guid UserId { get; set; }

    public string Permission { get; set; } = null!;

    public override (Guid, string) GetId() => (UserId, Permission);
}