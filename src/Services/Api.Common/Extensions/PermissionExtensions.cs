using Api.Common.EndPointFilters;
using Api.Common.Enums;
using Api.Common.Models;
using Core.Extensions;

namespace Api.Common.Extensions;

public static class PermissionExtensions
{
    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new RequiredPermissionsMetadata(permissions, requireAll: false));
        });

        builder.AddEndpointFilter(new PermissionFilter(PermissionCheck.Any, permissions));

        return builder;
    }

    public static TBuilder RequireAllPermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new RequiredPermissionsMetadata(permissions, requireAll: true));
        });

        builder.AddEndpointFilter(new PermissionFilter(PermissionCheck.All, permissions));

        return builder;
    }
    
    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params Enum[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        var perm = permissions.Select(x => x.ToNormalizedPermission()).ToArray();
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new RequiredPermissionsMetadata(perm, requireAll: false));
        });

        builder.AddEndpointFilter(new PermissionFilter(PermissionCheck.Any, perm));

        return builder;
    }

    public static TBuilder RequireAllPermissions<TBuilder>(this TBuilder builder, params Enum[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        var perm = permissions.Select(x => x.ToNormalizedPermission()).ToArray();
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new RequiredPermissionsMetadata(perm, requireAll: true));
        });

        builder.AddEndpointFilter(new PermissionFilter(PermissionCheck.All, perm));

        return builder;
    }
}