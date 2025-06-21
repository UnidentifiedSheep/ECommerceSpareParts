namespace MonoliteUnicorn.Dtos.Amw.Users;

public class AmwNewUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public HashSet<string> Roles { get; set; } = [];
}