using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionNotFoundException : 
    NotFoundException
{
    public PermissionNotFoundException(string name) : base("Не удалось найти разрешение", new { Name = name })
    {
    }
}