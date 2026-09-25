using Abstractions.Interfaces;
using Exceptions.Base;
using Microsoft.AspNetCore.Http;
using Security.Extensions;

namespace Security.Services;

public sealed class UserContext : IUserContext
{
	private readonly Guid? _userId;

	public UserContext(IHttpContextAccessor accessor)
	{
		var principal = accessor.GetPrincipal();

		IsAuthenticated = principal?.Identity?.IsAuthenticated == true;

		if (!IsAuthenticated)
		{
			_userId = null;
			Roles = new HashSet<string>();
			Permissions = new HashSet<string>();
			return;
		}

		var authenticatedPrincipal = principal!;
		_userId = authenticatedPrincipal.GetUserId();
		Roles = authenticatedPrincipal.GetRoles();
		Permissions = authenticatedPrincipal.GetPermissions();
	}

	public bool IsAuthenticated { get; }

	public Guid UserId =>
		IsAuthenticated && _userId.HasValue
			? _userId.Value
			: throw new UnauthorizedException("Пользователь не авторизован.");

	public Guid? UserIdOrNull => _userId;

	public IReadOnlySet<string> Roles { get; }

	public IReadOnlySet<string> Permissions { get; }

	public bool HasRole(string role) => Roles.Contains(role);

	public bool HasPermission(string permission) => Permissions.Contains(permission);
}
