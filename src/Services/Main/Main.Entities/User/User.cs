using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Main.Entities.Auth;
using Main.Entities.User.ValueObjects;

namespace Main.Entities.User;

public class User : AuditableEntity<User, Guid>
{
    [Validate]
    public Guid Id { get; private set; }
    public UserName UserName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public virtual UserInfo? UserInfo { get; private set; }
    public virtual UserDiscount? Discount { get; private set; }
    public virtual ICollection<UserEmail> UserEmails { get; private set; } = new List<UserEmail>();
    public virtual ICollection<UserPermission> UserPermissions { get; private set; } = new List<UserPermission>();
    public virtual ICollection<UserPhone> UserPhones { get; private set; } = new List<UserPhone>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<UserVehicle> UserVehicles { get; private set; } = new List<UserVehicle>();
    public virtual ICollection<Cart.Cart> CartItems { get; private set; } = new List<Cart.Cart>();

    public void SetDiscount(decimal discount)
    {
        if (Discount == null)
            Discount = UserDiscount.Create(Id, discount);
        else
            Discount.SetDiscount(discount);
    }
    
    public override Guid GetId() => Id;
}