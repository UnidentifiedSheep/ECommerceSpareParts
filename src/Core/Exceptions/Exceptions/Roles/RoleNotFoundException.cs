using Exceptions.Base;

namespace Exceptions.Exceptions.Roles;

public class RoleNotFoundException : NotFoundException
{
    public RoleNotFoundException(Guid id) : base("Не удалось найти роль", new { Id = id })
    {
    }

    public RoleNotFoundException(IEnumerable<Guid> ids) : base("Не удалось найти роли", new { Ids = ids })
    {
    }

    public RoleNotFoundException(string roleName) : base("Не удалось найти роль", new { Name = roleName })
    {
    }

    public RoleNotFoundException(IEnumerable<string> rolesNames) : base("Не удалось найти роли",
        new { Names = rolesNames })
    {
    }
}