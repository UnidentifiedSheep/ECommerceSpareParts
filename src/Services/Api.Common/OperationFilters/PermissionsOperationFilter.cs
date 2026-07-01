using Api.Common.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Common.OperationFilters;

public class PermissionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var permissionsMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<RequiredPermissionsMetadata>()
            .FirstOrDefault();

        if (permissionsMetadata != null)
        {
            var permissions = string.Join(", ", permissionsMetadata.Permissions);
            var type = permissionsMetadata.RequireAll ? "ALL" : "ANY";

            operation.Description +=
                $"\n\n**Required Permissions ({type}):** {permissions}";
        }

        var rolesMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<RequiredRolesMetadata>()
            .FirstOrDefault();

        if (rolesMetadata != null)
        {
            var roles = string.Join(", ", rolesMetadata.Roles);
            var type = rolesMetadata.RequireAll ? "ALL" : "ANY";

            operation.Description +=
                $"\n\n**Required Roles ({type}):** {roles}";
        }
    }
}
