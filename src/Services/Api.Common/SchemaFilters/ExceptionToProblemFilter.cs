using System.Text.Json;
using System.Text.Json.Nodes;
using Core.Abstractions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Common.SchemaFilters;

public class ExceptionToProblemFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema origSchema) return;
        
        ApplyExample(origSchema, context.Type);
    } 
    
    private void ApplyExample(OpenApiSchema origSchema, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(BaseExceptionExample))) return;
        if (Activator.CreateInstance(exceptionType) is not BaseExceptionExample problem) return; 
        
        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        origSchema.Examples ??= new List<JsonNode>();
        origSchema.Example = json;
        origSchema.Examples.Add(json);
    }
}