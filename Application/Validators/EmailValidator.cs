using System.Text.RegularExpressions;
using Core.Interfaces;

namespace Application.Validators;

public partial class EmailValidator : IEmailValidator
{
    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

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

        if (local.Length == 0 || local.Length > 64)
            return false;

        if (domain.Length == 0 || domain.Length > 255)
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
}