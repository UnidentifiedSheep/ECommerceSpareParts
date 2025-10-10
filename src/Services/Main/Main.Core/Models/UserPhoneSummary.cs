using Main.Core.Entities;

namespace Main.Core.Models;

public class UserPhoneSummary
{
    public Guid UserId { get; set; }
    public int PhoneCount { get; set; }
    public UserPhone? PrimaryPhone { get; set; }
}