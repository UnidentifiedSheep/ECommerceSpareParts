using Exceptions.Base;

namespace Exceptions.Exceptions.Permissions;

public class PermissionAlreadyExistsException(string name) 
    : BadRequestException("Данное разрешение уже существует.", new {Name = name}) { }