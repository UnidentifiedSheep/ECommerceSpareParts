using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Roles;

public class RoleAlreadyExistsException : BadRequestException
{
    [ExampleExceptionValues(false, "Example_Role_Name")]
    public RoleAlreadyExistsException(string roleName) : base("Роль уже существует", new { Name = roleName })
    {
    }
}