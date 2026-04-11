using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities;

public class User : AuditableEntity<User, Guid>
{
    [Validate]
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    [Validate]
    public string NormalizedUserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public virtual UserInfo? UserInfo { get; set; }
    public virtual UserDiscount? UserDiscount { get; set; }
    public virtual ICollection<UserEmail> UserEmails { get; set; } = new List<UserEmail>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public virtual ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
    public virtual ICollection<Cart> CartItems { get; set; } = new List<Cart>();
    public override Guid GetId() => Id;
}