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
    private readonly List<UserBalance> _balances = [];
    private readonly List<Cart.Cart> _cartItems = [];

    private readonly List<UserEmail> _emails = [];

    private readonly List<UserPermission> _permissions = [];

    private readonly List<UserPhone> _phones = [];

    private readonly List<UserRole> _roles = [];

    private readonly List<UserVehicle> _vehicles = [];

    private User() { }

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

    public static Expression<Func<User, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<User, bool>> GetEqualityExpression(Guid key) { return x => x.Id == key; }

    public static User Create(UserName userName, string passwordHash)
    {
        return new User(userName, passwordHash);
    }

    public void EnableTwoFactor(bool enabled) { TwoFactorEnabled = enabled; }

    public void SetUserInfo(
        string name,
        string surname,
        string? description)
    {
        if (UserInfo != null)
            UserInfo.Update(
                name,
                surname,
                description);
        else
            UserInfo = UserInfo.Create(
                Id,
                name,
                surname,
                description);
    }

    public void AddRole(string roleName)
    {
        if (_roles.Any(r => r.RoleName == RoleNames.Normalize(roleName))) return;
        _roles.Add(UserRole.Create(Id, roleName));
    }

    public void AddUserEmail(
        Email email,
        EmailType emailType,
        bool isPrimary,
        bool isConfirmed)
    {
        if (_emails.Any(x => x.Email.Value == email.Value))
            throw new InvalidInputException("user.have.duplicate.email");
        if (isPrimary && _emails.Any(x => x.IsPrimary))
            throw new InvalidInputException("user.email.primary.count");

        var userEmail = UserEmail.Create(
            Id,
            email,
            emailType);
        userEmail.MakePrimary(isPrimary);
        userEmail.Confirm(isConfirmed);
        _emails.Add(userEmail);
    }

    public void AddUserPhone(
        string phoneNumber,
        PhoneType phoneType,
        bool isPrimary,
        bool isConfirmed)
    {
        var normalizedPhone = UserPhone.ToNormalizedPhone(phoneNumber);
        if (_phones.Any(x => x.NormalizedPhone == normalizedPhone))
            throw new InvalidInputException("user.have.duplicate.phone");
        if (isPrimary && _phones.Any(x => x.IsPrimary))
            throw new InvalidInputException("user.phone.primary.count");

        var userPhone = UserPhone.Create(
            Id,
            phoneNumber,
            phoneType);
        userPhone.MakePrimary(isPrimary);
        userPhone.Confirm(isConfirmed);
        _phones.Add(userPhone);
    }

    public void RemoveUserPhone(string phoneNumber)
    {
        var normalizedPhone = UserPhone.ToNormalizedPhone(phoneNumber);
        _phones.RemoveAll(x => x.NormalizedPhone == normalizedPhone);
    }

    public void AddUserVehicle(
        Guid vehicleId,
        string plateNumber,
        string? vin = null,
        string? comment = null)
    {
        var normalizedPlateNumber = UserVehicle.NormalizePlateNumber(plateNumber);
        if (_vehicles.Any(x => x.PlateNumber == normalizedPlateNumber))
            throw new InvalidInputException("user.have.duplicate.vehicle.plate.number");

        var normalizedVin = UserVehicle.NormalizeVin(vin);
        if (normalizedVin != null && _vehicles.Any(x => x.Vin == normalizedVin))
            throw new InvalidInputException("user.have.duplicate.vehicle.vin.code");

        _vehicles.Add(
            UserVehicle.Create(
                Id,
                vehicleId,
                plateNumber,
                vin,
                comment));
    }

    public void RemoveUserVehicle(Guid vehicleId) { _vehicles.RemoveAll(x => x.VehicleId == vehicleId); }

    public void RemoveUserEmail(Email email) { _emails.RemoveAll(x => x.Email.Value == email.Value); }

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
            .EnsureNotNullOrWhiteSpace(() =>
                new InvalidOperationException("Password hash must not be null or empty."));
    }

    public void Login() { LastLoginAt = DateTime.UtcNow; }

    public override Guid GetId() { return Id; }
}