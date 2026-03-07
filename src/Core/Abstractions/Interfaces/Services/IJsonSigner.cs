namespace Abstractions.Interfaces.Services;

public interface IJsonSigner
{
    string Sign<T>(T data);
    bool VerifyJson(string signed, out string? json);
    bool VerifyJson<T>(string signed, out T? obj);
}