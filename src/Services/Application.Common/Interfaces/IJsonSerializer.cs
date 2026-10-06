using System.Diagnostics.CodeAnalysis;

namespace Application.Common.Interfaces;

public interface IJsonSerializer
{
	string Serialize<T>(T value);
	string Serialize(object? value, Type type);
	T? Deserialize<T>(string json);
	object? Deserialize(string json, Type type);
	bool TryDeserialize<T>(string json, [NotNullWhen(true)] out T? result);
	bool TryDeserialize(string json, Type type, [NotNullWhen(true)] out object? result);
}
