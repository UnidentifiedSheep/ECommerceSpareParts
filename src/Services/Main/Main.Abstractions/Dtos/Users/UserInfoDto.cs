namespace Main.Abstractions.Dtos.Users;

public class UserInfoDto
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public bool IsSupplier { get; set; }
    public string? Description { get; set; }
}