using System.Security.Cryptography;
using System.Text;
using Abstractions.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Security.Services;

public sealed class SecretEncryptor : ISecretEncryptor, IDisposable
{
    private const string Version = "v1";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly AesGcm _aesGcm;
    
    public SecretEncryptor(IOptions<SecretEncryptionOptions> options)
    {
        var key = Convert.FromBase64String(options.Value.Secret);

        if (key.Length != 32)
            throw new InvalidOperationException("Secret encryption key must be 32 bytes.");
        _aesGcm = new AesGcm(key, TagSize);
    }

    public string Encrypt(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var plaintext = Encoding.UTF8.GetBytes(value);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        _aesGcm.Encrypt(nonce, plaintext, cipher, tag);

        return string.Join('.',
            Version,
            Base64UrlEncode(nonce),
            Base64UrlEncode(cipher),
            Base64UrlEncode(tag));
    }

    public string Decrypt(string encrypted)
    {
        return TryDecrypt(encrypted, out var value) 
            ? value! 
            : throw new CryptographicException("Unable to decrypt secret.");
    }

    public bool TryDecrypt(string encrypted, out string? value)
    {
        value = null;

        try
        {
            if (string.IsNullOrWhiteSpace(encrypted))
                return false;

            var parts = encrypted.Split('.');
            if (parts.Length != 4 || parts[0] != Version)
                return false;

            var nonce = Base64UrlDecode(parts[1]);
            var cipher = Base64UrlDecode(parts[2]);
            var tag = Base64UrlDecode(parts[3]);

            if (nonce.Length != NonceSize || cipher.Length == 0 || tag.Length != TagSize)
                return false;

            var plaintext = new byte[cipher.Length];

            _aesGcm.Decrypt(nonce, cipher, tag, plaintext);

            value = Encoding.UTF8.GetString(plaintext);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        var padding = base64.Length % 4;
        base64 += padding switch
        {
            0 => string.Empty,
            2 => "==",
            3 => "=",
            _ => throw new FormatException("Invalid base64url length.")
        };

        return Convert.FromBase64String(base64);
    }

    public void Dispose()
    {
        _aesGcm.Dispose();
    }
}
