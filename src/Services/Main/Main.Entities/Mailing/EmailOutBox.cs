using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Enums;

namespace Main.Entities.Mailing;

public class EmailOutBox : AuditableEntity<EmailOutBox, Guid>, ILinqEntity<EmailOutBox, Guid>
{
    public Guid Id { get; private set; }
    public override Guid GetId() => Id;
    
    public string Subject { get; private set; }
    public string To { get; private set; }
    public string Body { get; private set; }
    public EmailStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }

    private EmailOutBox(
        string subject,
        string to,
        string body)
    {
        Subject = subject.AgainstNullOrWhiteSpace(() => new InvalidOperationException("Subject cannot be null or empty."));
        To = to.AgainstNullOrWhiteSpace(() => new InvalidOperationException("To cannot be null or empty."));
        Body = body.AgainstNullOrWhiteSpace(() => new InvalidOperationException("Body cannot be null or empty."));
        Status = EmailStatus.Pending;
    }

    public static EmailOutBox Create(
        string subject,
        string to,
        string body)
    {
        return new EmailOutBox(subject, to, body);
    }

    public void Sent()
    {
        if (Status != EmailStatus.Pending)
            throw new InvalidOperationException("Not pending email can not be sent.");
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        Body = string.Empty;
    }

    public void Cancelled()
    {
        if (Status != EmailStatus.Pending)
            throw new InvalidOperationException("Not pending email can not be cancelled.");
        Status = EmailStatus.Cancelled;
    }
    
    public static Expression<Func<EmailOutBox, Guid>> GetKeySelector()
        => x => x.Id;
    
    public static Expression<Func<EmailOutBox, bool>> GetEqualityExpression(Guid key)
        => x => x.Id == key;
}
