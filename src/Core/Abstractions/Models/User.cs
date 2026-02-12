namespace Abstractions.Models;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
}