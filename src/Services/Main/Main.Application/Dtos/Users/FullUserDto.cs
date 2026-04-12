namespace Main.Abstractions.Dtos.Users;

public class FullUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string PasswordHash { get; set; } = null!;
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public UserInfoDto? UserInfo { get; set; }
}