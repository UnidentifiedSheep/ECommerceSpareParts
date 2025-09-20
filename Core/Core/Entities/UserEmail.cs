namespace Core.Entities;

public partial class UserEmail
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string NormalizedEmail { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Confirmed { get; set; }

    public string EmailType { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
