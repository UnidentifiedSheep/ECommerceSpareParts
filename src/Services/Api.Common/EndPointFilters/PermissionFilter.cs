using Abstractions.Interfaces;
using Api.Common.Enums;

namespace Api.Common.EndPointFilters;

public sealed class PermissionFilter : IEndpointFilter
{
    private readonly PermissionCheck _check;
    private readonly string[] _permissions;

    public PermissionFilter(PermissionCheck check, params string[] permissions)
    {
        _permissions = permissions;
        _check = check;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var user = ctx.HttpContext.RequestServices.GetRequiredService<IUserContext>();

        if (!user.IsAuthenticated)
            return Results.Unauthorized();

        var allowed = _check == PermissionCheck.Any
            ? _permissions.Any(p => user.Permissions.Contains(p))
            : _permissions.All(p => user.Permissions.Contains(p));

        if (!allowed)
            return Results.Forbid();

        return await next(ctx);
    }
}