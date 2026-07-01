namespace Abstractions.Interfaces.Services;

public interface ISecretEncryptor
{
    string Encrypt(string value);
    string Decrypt(string encrypted);
    bool TryDecrypt(string encrypted, out string? value);
}