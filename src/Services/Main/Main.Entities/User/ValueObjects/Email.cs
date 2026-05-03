using System.Net.Mail;
using Domain.Extensions;

namespace Main.Entities.User.ValueObjects;

public record Email
{
    public string Value { get; } = null!;

    private Email() {}

    public Email(string value)
    {
        value = value.Trim().Against(z => !IsValid(z), "email.must.be.valid");
        Value = ToNormalized(value);
    }

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
    
    public static implicit operator Email(string value) => new(value);

    public static implicit operator string(Email email) => email.Value;
}