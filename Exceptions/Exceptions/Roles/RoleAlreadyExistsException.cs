using Exceptions.Base;

namespace Exceptions.Exceptions.Roles;

public class RoleAlreadyExistsException : BadRequestException
{
    public RoleAlreadyExistsException(string roleName) : base("Роль уже существует", new { Name = roleName })
    {
        
    }
}