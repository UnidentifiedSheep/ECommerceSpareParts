using System.Security.Claims;
using Abstractions.Interfaces;
using Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace Security.Services;

public sealed class UserContext : IUserContext
{
    private readonly Guid? _userId;

    public UserContext(IHttpContextAccessor accessor)
    {
        var principal = accessor.HttpContext?.User;

        IsAuthenticated = principal?.Identity?.IsAuthenticated == true;

        if (!IsAuthenticated)
        {
            _userId = null;
            Roles = new HashSet<string>();
            Permissions = new HashSet<string>();
            return;
        }

        _userId = GetUserId(principal!);
        Roles = GetRoles(principal!);
        Permissions = GetPermissions(principal!);
    }

    public bool IsAuthenticated { get; }

    public Guid UserId =>
        IsAuthenticated && _userId.HasValue
            ? _userId.Value
            : throw new UnauthorizedException("Пользователь не авторизован.");

    public Guid? UserIdOrNull => _userId;

    public IReadOnlySet<string> Roles { get; }

    public IReadOnlySet<string> Permissions { get; }

    public bool HasRole(string role) { return Roles.Contains(role); }

    public bool HasPermission(string permission) { return Permissions.Contains(permission); }

    private static Guid? GetUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }

    private static IReadOnlySet<string> GetRoles(ClaimsPrincipal principal)
    {
        return principal
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();
    }

    private static IReadOnlySet<string> GetPermissions(ClaimsPrincipal principal)
    {
        return principal
            .FindAll("permission")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();
    }
}