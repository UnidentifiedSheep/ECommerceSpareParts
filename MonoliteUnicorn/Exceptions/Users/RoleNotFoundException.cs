using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class RoleNotFoundException(IEnumerable<string> roles) : NotFoundException($"Не удалось найти найти роли ({string.Join(',', roles)})")
{
    
}