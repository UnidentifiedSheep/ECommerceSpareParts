using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.User.ValueObjects;
using Main.Enums;

namespace Main.Entities.User;

public class UserEmail : AuditableEntity<UserEmail, string>, ILinqEntity<UserEmail, string>
{
    private UserEmail() { }

    private UserEmail(
        Guid userId,
        Email email,
        EmailType emailType)
    {
        UserId = userId;
        Email = email;
        EmailType = emailType;
    }

    public Guid UserId { get; private set; }

    public Email Email { get; } = null!;

    public bool Confirmed { get; private set; }

    public EmailType EmailType { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }
    public User User { get; private set; } = null!;

    public static Expression<Func<UserEmail, string>> GetKeySelector() { return x => x.Email.Value; }

    public static Expression<Func<UserEmail, bool>> GetEqualityExpression(string key)
    {
        return x => x.Email == key;
    }

    internal static UserEmail Create(
        Guid userId,
        Email email,
        EmailType emailType)
    {
        return new UserEmail(
            userId,
            email,
            emailType);
    }

    public void Confirm(bool confirmed = true)
    {
        Confirmed = confirmed;
        ConfirmedAt = confirmed ? DateTime.UtcNow : null;
    }

    public void ChangeType(EmailType emailType) { EmailType = emailType; }

    public void MakePrimary(bool isPrimary = true) { IsPrimary = isPrimary; }

    public override string GetId() { return Email.Value; }
}