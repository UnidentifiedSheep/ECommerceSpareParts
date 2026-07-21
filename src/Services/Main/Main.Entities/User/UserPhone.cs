using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Extensions;
using Main.Enums;

namespace Main.Entities.User;

public partial class UserPhone : AuditableEntity<UserPhone, string>, ILinqEntity<UserPhone, string>
{
    public const int MinNormalizedPhoneLength = 7;
    public const int MaxNormalizedPhoneLength = 15;
    public const int MaxPhoneNumberLength = 32;

    private UserPhone() { }

    private UserPhone(
        Guid userId,
        string phoneNumber,
        PhoneType phoneType)
    {
        UserId = userId;
        SetPhoneNumber(phoneNumber);
        PhoneType = phoneType;
    }

    public Guid UserId { get; private set; }

    public string PhoneNumber { get; private set; } = null!;

    public string NormalizedPhone { get; private set; } = null!;

    public bool Confirmed { get; private set; }

    public bool IsPrimary { get; private set; }

    public PhoneType PhoneType { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }
    public User User { get; private set; } = null!;

    public static Expression<Func<UserPhone, string>> GetKeySelector() { return x => x.NormalizedPhone; }

    public static Expression<Func<UserPhone, bool>> GetEqualityExpression(string key)
    {
        return x => x.NormalizedPhone == key;
    }

    internal static UserPhone Create(
        Guid userId,
        string phoneNumber,
        PhoneType phoneType)
    {
        return new UserPhone(
            userId,
            phoneNumber,
            phoneType);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber
            .TrimSafe()
            .Ensure(IsValidPhone, "user.phone.invalid")
            .EnsureNotNullOrWhiteSpace("phone.number.required")
            .EnsureMaxLength(MaxPhoneNumberLength, "phone.number.max.length");

        NormalizedPhone = ToNormalizedPhone(phoneNumber)
            .EnsureNotNullOrWhiteSpace("phone.number.must.contain.digits")
            .EnsureMinLength(MinNormalizedPhoneLength, "phone.number.min.normalized.length")
            .EnsureMaxLength(MaxNormalizedPhoneLength, "phone.number.max.normalized.length");
    }

    public void Confirm(bool confirmed = true)
    {
        Confirmed = confirmed;
        ConfirmedAt = confirmed ? DateTime.UtcNow : null;
    }

    public void ChangeType(PhoneType phoneType) { PhoneType = phoneType; }

    public void MakePrimary(bool isPrimary = true) { IsPrimary = isPrimary; }

    public static string ToNormalizedPhone(string phoneNumber)
    {
        return phoneNumber.ToNormalizedPhoneNumber();
    }

    public bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;

        phone = phone.Trim();

        if (phone.Length is < 7 or > 20) return false;

        if (!PhoneRegex().IsMatch(phone)) return false;

        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        return digitsOnly.Length >= 7;
    }
    //Допустимые форматы
    // +1 (555) 123-4567
    // +44 20 7946 0958
    // (495) 123-45-67
    // 89001234567

    [GeneratedRegex(@"^\+?[0-9\s\-\(\)]{7,20}$")]
    private static partial Regex PhoneRegex();

    public override string GetId() { return NormalizedPhone; }
}
