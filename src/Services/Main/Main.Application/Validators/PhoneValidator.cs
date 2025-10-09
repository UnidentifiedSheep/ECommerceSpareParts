using System.Text.RegularExpressions;
using Core.Interfaces.Validators;

namespace Main.Application.Validators;

public partial class PhoneValidator : IPhoneValidator
{
    public bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        phone = phone.Trim();

        if (phone.Length < 7 || phone.Length > 20)
            return false;

        if (!PhoneRegex().IsMatch(phone))
            return false;

        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length < 7)
            return false;

        return true;
    }
    //Допустимые форматы
    // +1 (555) 123-4567
    // +44 20 7946 0958
    // (495) 123-45-67
    // 89001234567

    [GeneratedRegex(@"^\+?[0-9\s\-\(\)]{7,20}$")]
    private static partial Regex PhoneRegex();
}