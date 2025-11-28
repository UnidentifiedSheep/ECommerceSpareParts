using Api.Common.Enums;

namespace Api.Common.EndPointFilters;

public class PermissionFilter : IEndpointFilter
{
    private readonly string[] _permissions;
    private readonly PermissionCheck _check;

    public PermissionFilter(PermissionCheck check, params string[] permissions)
    {
        _permissions = permissions;
        _check = check;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var user = ctx.HttpContext.User;
        var userPerms = user.FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet();

        bool allowed = _check == PermissionCheck.Any
            ? _permissions.Any(p => userPerms.Contains(p))
            : _permissions.All(p => userPerms.Contains(p));
        
        if (!allowed)
            return Results.Forbid();

        return await next(ctx);
    }
}