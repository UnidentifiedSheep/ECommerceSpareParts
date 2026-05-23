using Exceptions.Base.Localized;
using Main.Enums;

namespace Main.Entities.Exceptions.Auth;

public class InvalidTokenException(string token)
    : LocalizedBadRequestException("invalid.token", new { Token = token });

public class PermissionNotFoundException(string name)
    : LocalizedNotFoundException("permission.not.found", new { Name = name }, [name]);

public class RoleAlreadyExistsException(string roleName)
    : LocalizedBadRequestException("role.already.exists", new { Name = roleName }, [roleName]);

public class RoleNotFoundException : LocalizedNotFoundException
{
    public RoleNotFoundException(Guid id)
        : base("role.not.found", new { Id = id })
    {
    }

    public RoleNotFoundException(string roleName)
        : base("role.not.found.with.role.name", new { Name = roleName }, [roleName])
    {
    }
}

public class UserAlreadyContainsRoleException(Guid userId, string role)
    : LocalizedConflictException("user.already.have.this.role", new { UserId = userId, Role = role }, [role]);

public class UserIsNotInNeededRole(Role role)
    : LocalizedBadRequestException(
        "user.is.not.in.needed.role",
        new { Role = role.ToString() },
        [role.ToString()]);

public class UserNotFoundException(Guid id)
    : LocalizedNotFoundException("user.not.found", new { Id = id });

public class WrongCredentialsException(string? email, string? password)
    : LocalizedBadRequestException("wrong.credentials", new { Email = email, Password = password });
