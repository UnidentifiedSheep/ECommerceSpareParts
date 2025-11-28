namespace Main.Core.Entities;

public partial class UserPermission
{
    public Guid UserId { get; set; }

    public string Permission { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Permission PermissionNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
