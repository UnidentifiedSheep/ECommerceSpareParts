using System.Net.Mail;
using Domain.Extensions;

namespace Main.Entities.User.ValueObjects;

public record Email
{
    private Email()
    {
    }

    public Email(string value)
    {
        value = value.Trim().Against(z => !IsValid(z), "email.must.be.valid");
        Value = ToNormalized(value);
    }

    public string Value { get; } = null!;

    public static string ToNormalized(string source)
    {
        return source.Trim().ToLowerInvariant();
    }

    private static bool IsValid(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator Email(string value)
    {
        return new Email(value);
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }
}