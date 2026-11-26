using System.Text.Json;
using System.Text.Json.Serialization;

namespace Integrations.Armtek.Converters;

public sealed class TolerantListConverter<T> : JsonConverter<IReadOnlyList<T>>
{
	private const string ArrayPropertyName = "ARRAY";

	public override bool HandleNull => true;

	public override IReadOnlyList<T> Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartArray)
			return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? [];

		if (reader.TokenType == JsonTokenType.StartObject)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			if (document.RootElement.TryGetProperty(ArrayPropertyName, out var array))
				return ReadArrayProperty(array, options);
		}
		else
			reader.Skip();

		// TODO: verify the RESP format Armtek returns for an empty search result.
		return [];
	}

	public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
		=> JsonSerializer.Serialize(writer, value, options);

	private static IReadOnlyList<T> ReadArrayProperty(JsonElement element, JsonSerializerOptions options)
		=> element.ValueKind switch
		{
			JsonValueKind.Array => element.Deserialize<List<T>>(options) ?? [],
			JsonValueKind.Object => element.Deserialize<T>(options) is { } item ? [item] : [],
			_ => []
		};
}
