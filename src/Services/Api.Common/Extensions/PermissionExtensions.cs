using Api.Common.Models;
using Security.Authorization;

namespace Api.Common.Extensions;

public static class PermissionExtensions
{
    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddPermissionRequirement(builder, permissions, AuthorizationMatch.Any);
    }

    public static TBuilder RequireAllPermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddPermissionRequirement(builder, permissions, AuthorizationMatch.All);
    }

    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params Enum[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddPermissionRequirement(
            builder,
            permissions.Select(x => x.ToString()),
            AuthorizationMatch.Any);
    }

    public static TBuilder RequireAllPermissions<TBuilder>(this TBuilder builder, params Enum[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddPermissionRequirement(
            builder,
            permissions.Select(x => x.ToString()),
            AuthorizationMatch.All);
    }

    public static TBuilder RequireAnyRole<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddRoleRequirement(builder, roles, AuthorizationMatch.Any);
    }

    public static TBuilder RequireAllRoles<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddRoleRequirement(builder, roles, AuthorizationMatch.All);
    }

    public static TBuilder RequireAnyRole<TBuilder>(this TBuilder builder, params Enum[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddRoleRequirement(
            builder,
            roles.Select(x => x.ToString()),
            AuthorizationMatch.Any);
    }

    public static TBuilder RequireAllRoles<TBuilder>(this TBuilder builder, params Enum[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return AddRoleRequirement(
            builder,
            roles.Select(x => x.ToString()),
            AuthorizationMatch.All);
    }

    private static TBuilder AddPermissionRequirement<TBuilder>(
        TBuilder builder,
        IEnumerable<string> permissions,
        AuthorizationMatch match)
        where TBuilder : IEndpointConventionBuilder
    {
        var requirement = new PermissionRequirement(permissions, match);
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(
                new RequiredPermissionsMetadata(
                    requirement.Permissions.ToArray(),
                    match == AuthorizationMatch.All));
        });
        builder.RequireAuthorization(policy => policy.AddRequirements(requirement));

        return builder;
    }

    private static TBuilder AddRoleRequirement<TBuilder>(
        TBuilder builder,
        IEnumerable<string> roles,
        AuthorizationMatch match)
        where TBuilder : IEndpointConventionBuilder
    {
        var requirement = new RoleRequirement(roles, match);
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(
                new RequiredRolesMetadata(
                    requirement.Roles.ToArray(),
                    match == AuthorizationMatch.All));
        });
        builder.RequireAuthorization(policy => policy.AddRequirements(requirement));

        return builder;
    }
}
