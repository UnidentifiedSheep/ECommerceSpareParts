using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Abstractions.Models.Options;
using Application.Common.Interfaces;

namespace Application.Common.Services;

public sealed class ProjectJsonSerializer(ProjectJsonOptions projectOptions) : IJsonSerializer
{
	public string Serialize<T>(T value)
		=> JsonSerializer.Serialize(value, projectOptions.SerializerOptions);

	public string Serialize(object? value, Type type)
		=> JsonSerializer.Serialize(value, type, projectOptions.SerializerOptions);

	public T? Deserialize<T>(string json)
		=> JsonSerializer.Deserialize<T>(json, projectOptions.SerializerOptions);

	public object? Deserialize(string json, Type type)
		=> JsonSerializer.Deserialize(json, type, projectOptions.SerializerOptions);

	public bool TryDeserialize<T>(string json, [NotNullWhen(true)] out T? result)
	{
		try
		{
			result = Deserialize<T>(json);
			return result is not null;
		}
		catch (JsonException)
		{
			result = default;
			return false;
		}
	}

	public bool TryDeserialize(string json, Type type, [NotNullWhen(true)] out object? result)
	{
		try
		{
			result = Deserialize(json, type);
			return result is not null;
		}
		catch (JsonException)
		{
			result = null;
			return false;
		}
	}
}
