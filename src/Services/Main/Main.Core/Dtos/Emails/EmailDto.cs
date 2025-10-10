using Main.Core.Enums;

namespace Main.Core.Dtos.Emails;

public class EmailDto
{
    public string Email { get; set; } = null!;
    public bool IsConfirmed { get; set; }
    public bool IsPrimary { get; set; }
    public EmailType Type { get; set; }
}