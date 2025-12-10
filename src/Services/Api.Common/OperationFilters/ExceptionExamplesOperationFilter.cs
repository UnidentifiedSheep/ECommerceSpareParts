using System.Text.Json.Nodes;
using Exceptions.Base;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Common.OperationFilters;

public class ExceptionExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null) return;

        var responseTypes = context.ApiDescription.SupportedResponseTypes;

        foreach (var responseType in responseTypes)
        {
            var statusCode = responseType.StatusCode;
            var type = responseType.Type;

            if (type == null || !typeof(BaseValuedException).IsAssignableFrom(type))
                continue;

            if (!operation.Responses.TryGetValue(statusCode.ToString(), out var response))
                continue;

            if (response.Content == null)
                continue;

            if (!response.Content.TryGetValue("application/json", out var mediaType))
                continue;

            mediaType.Examples ??= new Dictionary<string, IOpenApiExample>();

            if (!context.SchemaRepository.Schemas.TryGetValue(type.Name, out var schema))
                continue;


            if (schema.Example != null)
                mediaType.Examples.TryAdd(
                    $"{type.Name}_example",
                    new OpenApiExample
                    {
                        Value = schema.Example
                    });
            
            
            if (schema.Examples is not List<JsonNode> list) continue;
            int counter = 1;

            foreach (var exampleNode in list)
            {
                mediaType.Examples.TryAdd(
                    $"{type.Name}_example_{counter++}",
                    new OpenApiExample
                    {
                        Value = exampleNode
                    });
            }
        }
    }
}
