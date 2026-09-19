using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Locan.Core.Interfaces;
using static BCrypt.Net.BCrypt;

namespace Security.Services;

public class PasswordManager(PasswordRules rules) : IPasswordManager
{
	public string GetHashOfPassword(string password) => HashPassword(password);

	public bool VerifyHashedPassword(string hashedPassword, string providedPassword) =>
		Verify(providedPassword, hashedPassword);

	public (bool isValid, IEnumerable<ILocalizableMessage> errors) IsPasswordMatchRules(string password)
	{
		var errors = new List<ILocalizableMessage>();

		if (string.IsNullOrEmpty(password))
		{
			errors.Add(PasswordMustNotBeEmptyMessage.Instance);
			return (false, errors);
		}

		// Проверки длины сразу
		if (password.Length < rules.MinLength)
			errors.Add(new PasswordMinLengthMessage().WithMinLength(rules.MinLength));

		if (rules.MaxLength.HasValue && password.Length > rules.MaxLength.Value)
			errors.Add(new PasswordMaxLengthMessage().WithMaxLength(rules.MaxLength.Value));

		if (!rules.CanContainTrailingSpaces && (password[0] == ' ' || password[^1] == ' '))
			errors.Add(PasswordCannotStartOrEndWithSpaceMessage.Instance);

		var hasUpper = false;
		var hasDigit = false;
		var hasSpecial = false;
		var hasSpace = false;

		var specials = rules.SpecialCharacters;

		foreach (var c in password)
			if (char.IsUpper(c))
				hasUpper = true;
			else if (char.IsDigit(c))
				hasDigit = true;
			else if (specials.Contains(c))
				hasSpecial = true;
			else if (c == ' ')
				hasSpace = true;

		if (!rules.CanContainSpaces && hasSpace)
			errors.Add(PasswordCannotContainSpacesMessage.Instance);

		if (rules.RequireUppercase && !hasUpper)
			errors.Add(PasswordMustContainUppercaseMessage.Instance);

		if (rules.RequireDigit && !hasDigit)
			errors.Add(PasswordMustContainDigitMessage.Instance);

		if (rules.RequireSpecial && !hasSpecial)
			errors.Add(
				new PasswordMustContainSpecialMessage().WithSpecialCharacters(string.Join(',', specials)));

		return (errors.Count == 0, errors);
	}
}
