using System.Diagnostics.CodeAnalysis;

namespace Security.Core.Interfaces;

public interface IJsonSigner
{
	string Sign<T>(T data);

	bool VerifyJson(string signed, out string? json);

	bool VerifyJson<T>(string signed, [NotNullWhen(true)] out T? obj);
}
