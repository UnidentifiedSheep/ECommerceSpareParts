using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Users;

public class RoleNotFoundException(IEnumerable<string> roles) : NotFoundException($"Не удалось найти найти роли", new { Roles = roles })
{
    
}