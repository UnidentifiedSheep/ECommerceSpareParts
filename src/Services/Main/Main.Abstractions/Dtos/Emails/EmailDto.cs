using Main.Enums;

namespace Main.Abstractions.Dtos.Emails;

public class EmailDto
{
    public string Email { get; set; } = null!;
    public bool IsConfirmed { get; set; }
    public bool IsPrimary { get; set; }
    public EmailType Type { get; set; }
}