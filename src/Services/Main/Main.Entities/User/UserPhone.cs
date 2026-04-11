using Domain;

namespace Main.Entities;

public class UserPhone : AuditableEntity<UserPhone,  string>
{
    public Guid UserId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string NormalizedPhone { get; set; } = null!;

    public bool Confirmed { get; set; }

    public bool IsPrimary { get; set; }

    public string? PhoneType { get; set; }

    public DateTime? ConfirmedAt { get; set; }
    public override string GetId() => NormalizedPhone;
}