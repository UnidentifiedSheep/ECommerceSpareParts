using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Api.Common.Extensions;
using Core.Attributes;
using Core.Interfaces.Exceptions;
using Exceptions.Base;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Common.SchemaFilters;

public class ExceptionToProblemFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema origSchema) return;
        var ctors = context.Type
            .GetConstructors()
            .Select(x => (x, x.GetCustomAttribute<ExampleExceptionValuesAttribute>()))
            .Where(x => x.Item2 != null)
            .ToList();

        if (ctors.Count == 0) return;

        foreach (var (ctor, attr) in ctors)
            ApplyExample(origSchema, ctor, attr!, context.Type);
        
        
        origSchema.Type = JsonSchemaType.Object;
        origSchema.Properties?.Clear();
        origSchema.AdditionalPropertiesAllowed = false;

        SetDefaultExceptionProperties(origSchema);
    } 
    
    private void ApplyExample(OpenApiSchema origSchema, ConstructorInfo ctor, ExampleExceptionValuesAttribute attr, Type exceptionType)
    {
        var parameterType = ctor.GetParameters().FirstOrDefault()?.ParameterType; 
        object[] ctorParams;

        if (parameterType != null && parameterType != typeof(string) &&
            typeof(IEnumerable).IsAssignableFrom(parameterType))
        {
            var elementType = parameterType.GenericTypeArguments.FirstOrDefault() ?? typeof(object); 
            var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!; 
            foreach (var p in attr.Params) 
                typedList.Add(Convert.ChangeType(p, elementType)); 
            ctorParams = [typedList];
        } 
        else if (attr is { IsArray: false, Params: [Type exampleType] } && typeof(IExceptionExample).IsAssignableFrom(exampleType))
        {
            var example = Activator.CreateInstance(exampleType)!;
            var properties = example.GetType().GetProperties();
            var values = new object[properties.Length];

            for (int i = 0; i < properties.Length; i++)
                values[i] = properties[i].GetValue(example)!;
            ctorParams = values;
        }
        else
            ctorParams = attr.Params; 
        if (Activator.CreateInstance(exceptionType, ctorParams) is not BaseValuedException problem) return; 
        var errorRelated = problem.GetErrorValues(); 
        
        var json = new JsonObject
        {
            ["title"] = exceptionType.Name,
            ["detail"] = problem.Message,
            ["status"] = problem.GetStatusCode(),
            ["instance"] = "/example",
            ["type"] = "https://httpstatuses.io/",
            ["traceId"] = "example-trace-id",
            ["errorRelatedData"] = ToJsonNode(errorRelated)
        };
        origSchema.Examples ??= new List<JsonNode>();
        origSchema.Examples.Add(json);
    }


    private void SetDefaultExceptionProperties(OpenApiSchema schema)
    {
        schema.Properties?["title"] = new OpenApiSchema { Type = JsonSchemaType.String };
        schema.Properties?["detail"] = new OpenApiSchema { Type = JsonSchemaType.String };
        schema.Properties?["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" };
        schema.Properties?["instance"] = new OpenApiSchema { Type = JsonSchemaType.String };
        schema.Properties?["type"] = new OpenApiSchema { Type = JsonSchemaType.String };
        schema.Properties?["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String };
        schema.Properties?["errorRelatedData"] = new OpenApiSchema
        {
            OneOf = new List<IOpenApiSchema> 
            {
                new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    AdditionalProperties = new OpenApiSchema()
                },
                new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>()
                        {
                            ["property1"] = new OpenApiSchema { Type = JsonSchemaType.Object | JsonSchemaType.Array },
                            ["property2"] = new OpenApiSchema { Type = JsonSchemaType.Object | JsonSchemaType.Array }
                        }
                    }
                }
            }
        };
    }
    
    private static JsonNode? ToJsonNode(object? value)
    {
        if (value == null) return null;

        switch (value)
        {
            case JsonNode node:
                return node;
            case string s:
                return JsonValue.Create(s);
            case int i:
                return JsonValue.Create(i);
            case bool b:
                return JsonValue.Create(b);
            case IDictionary<string, object?> dict:
                var jsonDict = new JsonObject();
                foreach (var kv in dict)
                    jsonDict[kv.Key] = ToJsonNode(kv.Value);
                return jsonDict;
            case IEnumerable list:
                var array = new JsonArray();
                foreach (var item in list)
                    array.Add(ToJsonNode(item));
                return array;
            default:
                var props = value.GetType().GetProperties();
                if (props.Length > 0)
                {
                    var jsonObj = new JsonObject();
                    foreach (var prop in props)
                        jsonObj[prop.Name] = ToJsonNode(prop.GetValue(value));
                    return jsonObj;
                }
                return JsonValue.Create(value.ToString());
        }
    }

}