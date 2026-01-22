using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Users;

public class FullEmailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public bool Confirmed { get; set; }
    public EmailType EmailType { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}