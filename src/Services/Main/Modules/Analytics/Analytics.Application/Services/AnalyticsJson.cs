using System.Text.Json;
using System.Text.Json.Serialization;

namespace Main.Modules.Analytics.Application.Services;

public static class AnalyticsJson
{
	public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

	public static T Deserialize<T>(string json)
		=> JsonSerializer.Deserialize<T>(json, Options) ??
			throw new JsonException($"Value of type '{typeof(T).Name}' cannot be null.");
}
