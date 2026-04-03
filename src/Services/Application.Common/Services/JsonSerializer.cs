using Abstractions.Interfaces;

using SysJsonSerializer = System.Text.Json.JsonSerializer;
using SysJsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace Application.Common.Services;

public class JsonSerializer : IJsonSerializer
{
    private static readonly SysJsonSerializerOptions Options = SysJsonSerializerOptions.Web;
    
    public string Serialize<TValue>(TValue value)
    {
        return SysJsonSerializer.Serialize(value, Options);
    }

    public string Serialize(object value)
    {
        return SysJsonSerializer.Serialize(value, Options);
    }

    public TValue? Deserialize<TValue>(string value)
    {
        return SysJsonSerializer.Deserialize<TValue>(value, Options);
    }

    public object? Deserialize(string value, Type type)
    {
        return SysJsonSerializer.Deserialize(value, type, Options);
    }
}