using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionNotFoundException(string name) : 
    NotFoundException("Не удалось найти разрешение", new { Name = name }) { }