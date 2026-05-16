using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.User;

public class UserPhone : AuditableEntity<UserPhone, string>, ILinqEntity<UserPhone, string>
{
    public Guid UserId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string NormalizedPhone { get; set; } = null!;

    public bool Confirmed { get; set; }

    public bool IsPrimary { get; set; }

    public string? PhoneType { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public static Expression<Func<UserPhone, bool>> GetEqualityExpression(string key)
    {
        return x => x.NormalizedPhone == key;
    }

    public override string GetId()
    {
        return NormalizedPhone;
    }
}