using Core.Extensions;
using Core.Interfaces;
using Exceptions.Exceptions;

namespace Core.Models;

public sealed class Email
{
    public string FullEmail => $"{LocalPart}@{Domain}";
    public string NormalizedEmail => FullEmail.ToNormalized()!;
    public string LocalPart { get; }
    public string Domain { get; }
    
    private Email(string localPart, string domain)
    {
        LocalPart = localPart;
        Domain = domain.ToLowerInvariant();
    }

    public static Email Create(string email, IEmailValidator validator)
    {
        if (!validator.IsValidEmail(email))
            throw new EmailInvalidException(email);
        var splited = email.Split('@');
        var model = new Email(splited[0], splited[1]);
        return model;
    }
    
    public override bool Equals(object? obj) =>
        obj is Email other && string.Equals(NormalizedEmail, other.NormalizedEmail, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        NormalizedEmail.GetHashCode(StringComparison.OrdinalIgnoreCase);
    
    public override string ToString() => FullEmail;
}