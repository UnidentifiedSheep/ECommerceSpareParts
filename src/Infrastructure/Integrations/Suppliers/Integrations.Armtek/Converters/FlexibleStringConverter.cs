using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Integrations.Armtek.Converters;

public sealed class FlexibleStringConverter : JsonConverter<string?>
{
	public override bool HandleNull => true;

	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Null:
				return null;
			case JsonTokenType.String:
				return reader.GetString();
			case JsonTokenType.Number:
				return Encoding.UTF8.GetString(reader.ValueSpan);
			case JsonTokenType.True:
				return bool.TrueString;
			case JsonTokenType.False:
				return bool.FalseString;
			default:
				reader.Skip();
				return null;
		}
	}

	public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
	{
		if (value is null)
			writer.WriteNullValue();
		else
			writer.WriteStringValue(value);
	}
}
