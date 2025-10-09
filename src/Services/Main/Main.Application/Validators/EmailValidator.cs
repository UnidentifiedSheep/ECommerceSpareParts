using System.Text.RegularExpressions;
using Core.Interfaces.Validators;

namespace Main.Application.Validators;

public partial class EmailValidator : IEmailValidator
{
    public bool IsValidEmail(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return false;

        if (source.Length > 254)
            return false;

        var parts = source.Split('@');
        if (parts.Length != 2)
            return false;

        var local = parts[0];
        var domain = parts[1];

        if (local.Length is 0 or > 64)
            return false;

        if (domain.Length is 0 or > 255)
            return false;

        if (local.Contains("..") || domain.Contains(".."))
            return false;

        if (!domain.Contains('.'))
            return false;

        var domainLabels = domain.Split('.');
        if (domainLabels.Any(label => label.StartsWith('-') || label.EndsWith('-')))
            return false;

        return EmailRegex().IsMatch(source);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}