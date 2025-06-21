using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Json;

public class StringOrArrayConverter : JsonConverter<List<string>>
{
    public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        return token.Type switch
        {
            JTokenType.String => [token.ToString()],
            JTokenType.Array => token.ToObject<List<string>>(),
            _ => new List<string>()
        };
    }

    public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer)
    {
        if (value == null || value.Count == 0)
        {
            writer.WriteNull();
            return;
        }
        
        if (value.Count == 1)
            writer.WriteValue(value[0]);
        else
        {
            writer.WriteStartArray();
            foreach (var str in value)
            {
                writer.WriteValue(str);
            }
            writer.WriteEndArray();
        }
    }
}