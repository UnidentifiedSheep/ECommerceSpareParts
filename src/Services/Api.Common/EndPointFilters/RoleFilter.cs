using Abstractions.Interfaces;
using Api.Common.Enums;

namespace Api.Common.EndPointFilters;

public sealed class RoleFilter : IEndpointFilter
{
    private readonly PermissionCheck _check;
    private readonly string[] _roles;

    public RoleFilter(PermissionCheck check, params string[] roles)
    {
        _roles = roles;
        _check = check;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var user = ctx.HttpContext.RequestServices.GetRequiredService<IUserContext>();

        if (!user.IsAuthenticated) return Results.Unauthorized();

        var allowed = _check == PermissionCheck.Any
            ? _roles.Any(user.Roles.Contains)
            : _roles.All(user.Roles.Contains);

        if (!allowed) return Results.Forbid();

        return await next(ctx);
    }
}