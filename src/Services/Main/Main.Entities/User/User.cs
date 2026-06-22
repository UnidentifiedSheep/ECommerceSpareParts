using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Exceptions;
using Main.Entities.Auth;
using Main.Entities.Balance;
using Main.Entities.User.ValueObjects;
using Main.Enums;

namespace Main.Entities.User;

public class User : AuditableEntity<User, Guid>, ILinqEntity<User, Guid>
{
    private readonly List<Cart.Cart> _cartItems = [];

    private readonly List<UserEmail> _emails = [];

    private readonly List<UserPermission> _permissions = [];

    private readonly List<UserPhone> _phones = [];

    private readonly List<UserRole> _roles = [];

    private readonly List<UserVehicle> _vehicles = [];
    
    private readonly List<UserBalance> _balances = [];

    private User()
    {
    }

    private User(UserName userName, string passwordHash)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }

    [Validate]
    public Guid Id { get; private set; }

    public UserName UserName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public UserInfo? UserInfo { get; private set; }
    public UserDiscount? Discount { get; private set; }
    public UserFinancialProfile? FinancialProfile { get; private set; }
    public IReadOnlyList<UserEmail> Emails => _emails;
    public IReadOnlyList<UserPermission> Permissions => _permissions;
    public IReadOnlyList<UserPhone> Phones => _phones;
    public IReadOnlyList<UserRole> Roles => _roles;
    public IReadOnlyList<UserVehicle> Vehicles => _vehicles;
    public IReadOnlyList<Cart.Cart> CartItems => _cartItems;
    public IReadOnlyList<UserBalance> Balances => _balances;

    public static Expression<Func<User, Guid>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<User, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public static User Create(UserName userName, string passwordHash)
    {
        return new User(userName, passwordHash);
    }

    public void EnableTwoFactor(bool enabled)
    {
        TwoFactorEnabled = enabled;
    }

    public void SetUserInfo(string name, string surname, string? description)
    {
        if (UserInfo != null)
            UserInfo.Update(name, surname, description);
        else
            UserInfo = UserInfo.Create(Id, name, surname, description);
    }

    public void AddRole(string roleName)
    {
        if (_roles.Any(r => r.RoleName == RoleNames.Normalize(roleName))) return;
        _roles.Add(UserRole.Create(Id, roleName));
    }

    public void AddUserEmail(Email email, EmailType emailType, bool isPrimary, bool isConfirmed)
    {
        if (_emails.Any(x => x.Email.Value == email.Value))
            throw new InvalidInputException("user.have.duplicate.email");
        if (isPrimary && _emails.Any(x => x.IsPrimary))
            throw new InvalidInputException("user.email.primary.count");

        var userEmail = UserEmail.Create(Id, email, emailType);
        userEmail.MakePrimary(isPrimary);
        userEmail.Confirm(isConfirmed);
        _emails.Add(userEmail);
    }

    public void SetDiscount(decimal discount)
    {
        if (Discount == null)
            Discount = UserDiscount.Create(Id, discount);
        else
            Discount.SetDiscount(discount);
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash
            .AgainstNullOrWhiteSpace(() => new InvalidOperationException("Password hash must not be null or empty."));
    }

    public void Login()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public override Guid GetId()
    {
        return Id;
    }
}
