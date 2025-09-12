namespace Core.Dtos.Amw.Users;

public class NewUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    public string? Description { get; set; }
    public HashSet<string> Roles { get; set; } = [];
}