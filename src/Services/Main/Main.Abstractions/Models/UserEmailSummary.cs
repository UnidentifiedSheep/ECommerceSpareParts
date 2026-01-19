using Main.Entities;

namespace Main.Abstractions.Models;

public class UserEmailSummary
{
    public Guid UserId { get; set; }
    public int EmailCount { get; set; }
    public UserEmail? PrimaryEmail { get; set; }
}