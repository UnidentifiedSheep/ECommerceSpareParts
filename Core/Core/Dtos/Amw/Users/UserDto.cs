namespace Core.Dtos.Amw.Users;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public bool IsSupplier { get; set; }
    public string? Description { get; set; }
}