using Api.Common.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Common.OperationFilters;

public class PermissionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<RequiredPermissionsMetadata>()
            .FirstOrDefault();

        if (metadata == null)
            return;

        var permissions = string.Join(", ", metadata.Permissions);

        var type = metadata.RequireAll ? "ALL" : "ANY";

        operation.Description +=
            $"\n\n**Required Permissions ({type}):** {permissions}";
    }
}