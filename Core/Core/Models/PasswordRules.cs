namespace Core.Models;

public class PasswordRules
{
    public int MinLength { get; set; } = 8;
    public int? MaxLength { get; set; }
    public bool RequireUppercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecial { get; set; }
    public bool CanContainTrailingSpaces { get; set; } = false;
    public bool CanContainSpaces { get; set; } = false;
    public HashSet<char> SpecialCharacters { get; set; } = new("!@#$%^&*()-_=+[]{}|;:,.<>?");
}