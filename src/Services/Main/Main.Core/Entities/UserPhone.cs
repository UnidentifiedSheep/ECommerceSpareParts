namespace Main.Core.Entities;

public partial class UserPhone
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string NormalizedPhone { get; set; } = null!;

    public bool Confirmed { get; set; }

    public bool IsPrimary { get; set; }

    public string? PhoneType { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
