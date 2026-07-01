using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Attributes.JsonAttributes;
using Localization.Abstractions.Interfaces;

namespace Localization.Domain.Serialization;

public sealed class ScopedLocalizedJsonSerializer(
    IScopedStringLocalizer localizer
) : IScopedLocalizedJsonSerializer
{
    private static readonly ConcurrentDictionary<Type, JsonFieldPlan[]> MetadataCache = new();
    private static readonly ConcurrentDictionary<Type, CsvColumnPlan[]?> CsvSchemaCache = new();

    private static readonly JsonSerializerOptions MetadataOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new LocalizableResolver(localizer)
    };

    public string Serialize<T>(T value) { return JsonSerializer.Serialize(value, _options); }

    public string SerializeMetadata<T>() { return SerializeMetadata(typeof(T)); }

    public string SerializeMetadata(Type type)
    {
        var fields = MetadataCache
            .GetOrAdd(type, BuildFieldPlans)
            .Select(ToFieldMetadata)
            .ToArray();
        var csvSchema = CsvSchemaCache
            .GetOrAdd(type, BuildCsvSchemaPlan)
            ?
            .Select(ToCsvColumnMetadata)
            .ToArray();

        return JsonSerializer.Serialize(new JsonObjectMetadata(fields, csvSchema), MetadataOptions);
    }

    private static JsonFieldPlan[] BuildFieldPlans(Type type)
    {
        var typeInfo = MetadataOptions.GetTypeInfo(type);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            throw new InvalidOperationException($"Type {type.Name} is not a JSON object.");

        return typeInfo.Properties
            .Select(prop =>
            {
                var nameAttr = GetAttribute<LocalizedJsonFieldNameAttribute>(prop);
                var descriptionAttr = GetAttribute<LocalizedJsonFieldDescriptionAttribute>(prop);
                var inputControlAttr = GetAttribute<InputControlAttribute>(prop);
                var acceptsAttr = GetAttribute<AcceptsAttribute>(prop);
                var dependsOnEntityAttr = GetAttribute<DependsOnEntityAttribute>(prop);

                return new JsonFieldPlan(
                    prop.Name,
                    GetJsonType(prop.PropertyType),
                    nameAttr?.Key,
                    descriptionAttr?.Key,
                    GetAttribute<RequiredJsonFieldAttribute>(prop) is not null,
                    inputControlAttr?.InputControl.ToString(),
                    acceptsAttr?.Accepts ?? [],
                    dependsOnEntityAttr?.EntityName,
                    dependsOnEntityAttr?.FieldName);
            })
            .ToArray();
    }

    private static CsvColumnPlan[]? BuildCsvSchemaPlan(Type type)
    {
        var schemaAttr = type.GetCustomAttribute<CsvSchemaAttribute>();
        if (schemaAttr is null) return null;

        return schemaAttr.RowType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(prop =>
            {
                var nameAttr = prop.GetCustomAttributes(true)
                    .FirstOrDefault(x =>
                        x.GetType().FullName == "CsvHelper.Configuration.Attributes.NameAttribute");
                var aliases = GetCsvNameAliases(nameAttr);

                return new CsvColumnPlan(
                    prop.Name,
                    aliases.Length == 0 ? [prop.Name] : aliases,
                    GetJsonType(prop.PropertyType),
                    IsCsvColumnRequired(prop),
                    prop.GetCustomAttribute<LocalizedJsonFieldNameAttribute>()?.Key,
                    prop.GetCustomAttribute<LocalizedJsonFieldDescriptionAttribute>()?.Key);
            })
            .ToArray();
    }

    private JsonFieldMetadata ToFieldMetadata(JsonFieldPlan plan)
    {
        return new JsonFieldMetadata(
            plan.Name,
            plan.Type,
            GetLocalizedOrDefault(plan.LabelKey),
            GetLocalizedOrDefault(plan.DescriptionKey),
            plan.Required,
            plan.Control,
            plan.Accepts,
            plan.DependsOnEntity,
            plan.DependsOnField);
    }

    private CsvColumnMetadata ToCsvColumnMetadata(CsvColumnPlan plan)
    {
        return new CsvColumnMetadata(
            plan.PropertyName,
            plan.Names,
            plan.Type,
            plan.Required,
            GetLocalizedOrDefault(plan.LabelKey),
            GetLocalizedOrDefault(plan.DescriptionKey));
    }

    private string? GetLocalizedOrDefault(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var value = localizer.GetOrDefault(key);
        return string.IsNullOrWhiteSpace(value) ? key : value;
    }

    private static T? GetAttribute<T>(JsonPropertyInfo prop)
        where T : Attribute
    {
        return prop.AttributeProvider?
            .GetCustomAttributes(typeof(T), true)
            .OfType<T>()
            .FirstOrDefault();
    }

    private static string[] GetCsvNameAliases(object? nameAttribute)
    {
        if (nameAttribute is null) return [];

        var names = nameAttribute.GetType()
            .GetProperty("Names", BindingFlags.Public | BindingFlags.Instance)
            ?
            .GetValue(nameAttribute) as string[];

        return names ?? [];
    }

    private static bool IsCsvColumnRequired(PropertyInfo prop)
    {
        return prop.GetCustomAttributes(true)
            .All(x => x.GetType().FullName != "CsvHelper.Configuration.Attributes.OptionalAttribute");
    }

    private static string GetJsonType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) ||
            type == typeof(DateTimeOffset))
            return "string";
        if (type == typeof(bool)) return "boolean";
        if (type.IsEnum) return "enum";
        if (type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
            type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
            return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type)) return "array";

        return "object";
    }

    private sealed record JsonObjectMetadata(
        JsonFieldMetadata[] Fields,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        CsvColumnMetadata[]? CsvSchema
    );

    private sealed record JsonFieldPlan(
        string Name,
        string Type,
        string? LabelKey,
        string? DescriptionKey,
        bool Required,
        string? Control,
        string[] Accepts,
        string? DependsOnEntity,
        string? DependsOnField
    );

    private sealed record JsonFieldMetadata(
        string Name,
        string Type,
        string? Label,
        string? Description,
        bool Required,
        string? Control,
        string[] Accepts,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? DependsOnEntity,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? DependsOnField
    );

    private sealed record CsvColumnPlan(
        string PropertyName,
        string[] Names,
        string Type,
        bool Required,
        string? LabelKey,
        string? DescriptionKey
    );

    private sealed record CsvColumnMetadata(
        string PropertyName,
        string[] Names,
        string Type,
        bool Required,
        string? Label,
        string? Description
    );
}