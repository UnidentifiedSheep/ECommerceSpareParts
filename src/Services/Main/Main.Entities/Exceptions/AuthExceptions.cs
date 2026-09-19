using Enums;
using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class InvalidTokenException(string token) : LocalizedBadRequestException(
	InvalidTokenMessage.Instance,
	new
	{
		Token = token
	});

public class PermissionNotFoundException(string name) : LocalizedNotFoundException(
	new PermissionNotFoundMessage().WithName(name),
	new
	{
		Name = name
	});

public class RoleAlreadyExistsException(string roleName) : LocalizedBadRequestException(
	new RoleAlreadyExistsMessage().WithName(roleName),
	new
	{
		Name = roleName
	});

public class RoleNotFoundException : LocalizedNotFoundException
{
	public RoleNotFoundException(Guid id) : base(
		RoleNotFoundMessage.Instance,
		new
		{
			Id = id
		})
	{
	}

	public RoleNotFoundException(string roleName) : base(
		new RoleNotFoundWithRoleNameMessage().WithName(roleName),
		new
		{
			Name = roleName
		})
	{
	}
}

public class UserAlreadyContainsRoleException(Guid userId, string role) : LocalizedConflictException(
	new UserAlreadyHaveThisRoleMessage().WithRole(role),
	new
	{
		UserId = userId, Role = role
	});

public class UserRoleNotFoundException(Guid userId, string role) : LocalizedNotFoundException(
	new UserRoleNotFoundMessage().WithRole(role),
	new
	{
		UserId = userId, Role = role
	});

public class UserIsNotInNeededRole(Role role) : LocalizedBadRequestException(
	new UserIsNotInNeededRoleMessage().WithRole(role.ToString()),
	new
	{
		Role = role.ToString()
	});

public class UserNotFoundException(Guid id) : LocalizedNotFoundException(
	UserNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class WrongCredentialsException(string? login, string? password) : LocalizedBadRequestException(
	WrongCredentialsMessage.Instance,
	new
	{
		Login = login, Password = password
	});

public class UserEmailAlreadyInUseException(string email) : LocalizedConflictException(
	UserEmailAlreadyInUseMessage.Instance,
	new
	{
		Email = email
	});

public class UserEmailNotFoundException(string email) : LocalizedNotFoundException(
	UserEmailNotFoundMessage.Instance,
	new
	{
		Email = email
	});

public class CantCreateSystemUserException()
	: LocalizedBadRequestException(CantCreateSystemUserMessage.Instance);

public class ResetTokenExpiredException() : LocalizedBadRequestException(ResetTokenExpiredMessage.Instance);

public class EmailVerificationTokenExpiredException()
	: LocalizedBadRequestException(EmailVerificationTokenExpiredMessage.Instance);

public class UserPermissionNotFound(Guid id, string permission) : LocalizedNotFoundException(
	UserPermissionNotFoundMessage.Instance,
	new
	{
		UserId = id, Permission = permission
	});
