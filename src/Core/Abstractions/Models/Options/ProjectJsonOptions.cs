using System.Text.Json;
using System.Text.Json.Serialization;

namespace Abstractions.Models.Options;

public sealed class ProjectJsonOptions
{
    public ProjectJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configure(options);
        SerializerOptions = options;
    }

    public JsonSerializerOptions SerializerOptions { get; }

    public static void Configure(JsonSerializerOptions options)
    {
        if (options.Converters.All(converter => converter is not JsonStringEnumConverter))
            options.Converters.Add(new JsonStringEnumConverter());
    }
}