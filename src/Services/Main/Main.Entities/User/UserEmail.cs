using BulkValidation.Core.Attributes;
using Domain;
using Main.Enums;

namespace Main.Entities.User;

public class UserEmail : AuditableEntity<UserEmail, string>
{
    public Guid UserId { get; set; }

    [Validate]
    public string NormalizedEmail { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Confirmed { get; set; }

    public EmailType EmailType { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime? ConfirmedAt { get; set; }
    
    public override string GetId() => NormalizedEmail;
}