using Abstractions.Interfaces;
using Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace Security.Services;

public sealed class UserContext : IUserContext
{
    public bool IsAuthenticated { get; }

    private readonly Guid? _userId;

    public Guid UserId => IsAuthenticated && _userId.HasValue
            ? _userId.Value
            : throw new UnauthorizedAccessException("Пользователь не авторизован.");

    public IReadOnlySet<string> Roles { get; }
    public IReadOnlySet<string> Permissions { get; }

    public UserContext(IHttpContextAccessor accessor)
    {
        var headers = accessor.HttpContext!.Request.Headers;

        IsAuthenticated = headers.ContainsKey("X-User-Id");

        if (!IsAuthenticated)
        {
            _userId = null;
            Roles = new HashSet<string>();
            Permissions = new HashSet<string>();
            return;
        }

        _userId = Guid.TryParse(headers["X-User-Id"], out var id) ? id : null;

        Roles = headers["X-Roles"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        Permissions = headers["X-Permissions"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
    }
}