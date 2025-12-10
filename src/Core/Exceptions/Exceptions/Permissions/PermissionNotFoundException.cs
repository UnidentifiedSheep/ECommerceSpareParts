using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionNotFoundException : 
    NotFoundException
{
    [ExampleExceptionValues(false, "EXAMPLE_PERMISSION")]
    public PermissionNotFoundException(string name) : base("Не удалось найти разрешение", new { Name = name })
    {
    }
}