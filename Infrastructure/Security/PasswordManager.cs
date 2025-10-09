using static BCrypt.Net.BCrypt;
using Core.Interfaces.Validators;
using Core.Models;

namespace Security;

public class PasswordManager(PasswordRules rules) : IPasswordManager
{
    public string GetHashOfPassword(string password)
    {
        return HashPassword(password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        return Verify(providedPassword, hashedPassword);
    }

    public (bool isValid, IEnumerable<string> errors) IsPasswordMatchRules(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Пароль не должен быть пуст.");
            return (false, errors);
        }

        // Проверки длины сразу
        if (password.Length < rules.MinLength)
            errors.Add($"Пароль должен быть длиной минимум в {rules.MinLength} символов.");

        if (rules.MaxLength.HasValue && password.Length > rules.MaxLength.Value)
            errors.Add($"Пароль должен быть длиной максимум до {rules.MaxLength.Value} символов.");

        if (!rules.CanContainTrailingSpaces && 
            (password[0] == ' ' || password[^1] == ' '))
            errors.Add("Пароль не может начинаться или заканчиваться пробелом.");

        bool hasUpper = false;
        bool hasDigit = false;
        bool hasSpecial = false;
        bool hasSpace = false;

        HashSet<char> specials = rules.SpecialCharacters;
        
        foreach (var c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (specials.Contains(c)) hasSpecial = true;
            else if (c == ' ') hasSpace = true;
        }

        if (!rules.CanContainSpaces && hasSpace)
            errors.Add("Пароль не может содержать пробелы");

        if (rules.RequireUppercase && !hasUpper)
            errors.Add("Пароль должен содержать как минимум один заглавный символ");

        if (rules.RequireDigit && !hasDigit)
            errors.Add("Пароль должен содержать минимум одну цифру");

        if (rules.RequireSpecial && !hasSpecial)
            errors.Add($"Пароль должен содержать минимум один спец. символ ({new string(specials.ToArray())}).");

        return (errors.Count == 0, errors);
    }
}