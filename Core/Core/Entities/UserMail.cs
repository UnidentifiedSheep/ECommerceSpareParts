namespace Core.Entities;

public class UserMail
{
    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool IsVerified { get; set; }

    public string LocalPart { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}