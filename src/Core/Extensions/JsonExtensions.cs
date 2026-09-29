using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Extensions;

public static class JsonExtensions
{
	public static bool TryDeserializeJson<T>(
		this string json,
		[NotNullWhen(true)] out T? result,
		JsonSerializerOptions? options = null)
	{
		try
		{
			result = JsonSerializer.Deserialize<T>(json, options);
			return result is not null;
		}
		catch (JsonException)
		{
			result = default;
			return false;
		}
	}

	public static bool TryDeserializeJson(
		this string json,
		Type type,
		[NotNullWhen(true)] out object? result,
		JsonSerializerOptions? options = null)
	{
		try
		{
			result = JsonSerializer.Deserialize(json, type, options);
			return result is not null;
		}
		catch (JsonException)
		{
			result = null;
			return false;
		}
	}
}
