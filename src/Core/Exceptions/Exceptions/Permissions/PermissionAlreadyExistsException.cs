using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionAlreadyExistsException : BadRequestException
{
    public PermissionAlreadyExistsException(string name) : base("Данное разрешение уже существует.", new {Name = name})
    {
    }
}