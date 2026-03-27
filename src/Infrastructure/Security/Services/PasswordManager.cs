using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using static BCrypt.Net.BCrypt;

namespace Security.Services;

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

    public (bool isValid, IEnumerable<(string key, object[]? args)> errors) IsPasswordMatchRules(string password)
    {
        var errors = new List<(string, object[]?)>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add(("password.must.not.be.empty", null));
            return (false, errors);
        }

        // Проверки длины сразу
        if (password.Length < rules.MinLength)
            errors.Add(("password.min.length", [rules.MinLength]));

        if (rules.MaxLength.HasValue && password.Length > rules.MaxLength.Value)
            errors.Add(("password.max.length", [rules.MaxLength.Value]));

        if (!rules.CanContainTrailingSpaces &&
            (password[0] == ' ' || password[^1] == ' '))
            errors.Add(("password.cannot.start.or.end.with.space", null));

        var hasUpper = false;
        var hasDigit = false;
        var hasSpecial = false;
        var hasSpace = false;

        var specials = rules.SpecialCharacters;

        foreach (var c in password)
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (specials.Contains(c)) hasSpecial = true;
            else if (c == ' ') hasSpace = true;

        if (!rules.CanContainSpaces && hasSpace)
            errors.Add(("password.cannot.contain.spaces", null));

        if (rules.RequireUppercase && !hasUpper)
            errors.Add(("password.must.contain.uppercase", null));

        if (rules.RequireDigit && !hasDigit)
            errors.Add(("password.must.contain.digit", null));

        if (rules.RequireSpecial && !hasSpecial)
            errors.Add(("password.must.contain.special", [string.Join(',', specials)]));

        return (errors.Count == 0, errors);
    }
}