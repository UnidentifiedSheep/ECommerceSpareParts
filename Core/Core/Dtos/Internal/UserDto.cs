namespace Core.Dtos.Internal;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string? Email { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? UserName { get; set; }
}