namespace Main.Abstractions.Dtos.Users;

public record UserRolesAndPermissions
{
    public required IReadOnlyList<string> Roles { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
    
    public void Deconstruct(
        out IReadOnlyList<string> roles,
        out IReadOnlyList<string> permissions)
    {
        roles = Roles;
        permissions = Permissions;
    }
}