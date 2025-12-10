using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionAlreadyExistsException : BadRequestException
{
    [ExampleExceptionValues(false, "EXAMPLE_PERMISSION")]
    public PermissionAlreadyExistsException(string name) : base("Данное разрешение уже существует.", new {Name = name})
    {
    }
}