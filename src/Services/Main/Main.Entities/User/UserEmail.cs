using Domain;
using Main.Entities.User.ValueObjects;
using Main.Enums;

namespace Main.Entities.User;

public class UserEmail : AuditableEntity<UserEmail, string>
{
    public Guid UserId { get; private set; }

    public Email Email { get; private set; } = null!;

    public bool Confirmed { get; private set; }

    public EmailType EmailType { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }
    
    private UserEmail() {}

    private UserEmail(Guid userId, Email email, EmailType emailType)
    {
        UserId = userId;
        Email = email;
        EmailType = emailType;
    }

    internal static UserEmail Create(Guid userId, Email email, EmailType emailType)
    {
        return new UserEmail(userId, email, emailType);
    }

    public void Confirm(bool confirmed = true)
    {
        Confirmed = confirmed;
        ConfirmedAt = confirmed ? DateTime.UtcNow : null;
    }

    public void ChangeType(EmailType emailType)
    {
        EmailType = emailType;
    }

    public void MakePrimary(bool isPrimary = true)
    {
        IsPrimary = isPrimary;
    }
    
    public override string GetId() => Email.NormalizedValue;
}