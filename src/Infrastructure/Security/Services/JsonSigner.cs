using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Abstractions.Interfaces.Services;
using Utils;

namespace Security.Services;

public class JsonSigner : IJsonSigner
{
    private readonly JsonSerializerOptions? _options;
    private readonly byte[] _secretBytes;

    public JsonSigner(string secret, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        _options = options;
        _secretBytes = Encoding.UTF8.GetBytes(secret);
    }

    public string Sign<T>(T data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var json = JsonSerializer.Serialize(data, _options);
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        using var hmac = new HMACSHA256(_secretBytes);
        var hash = hmac.ComputeHash(jsonBytes);

        var payload = EncodingUtils.Base64UrlEncode(jsonBytes);
        var signature = EncodingUtils.Base64UrlEncode(hash);

        return $"{payload}.{signature}";
    }

    public bool VerifyJson(string signed, out string? json)
    {
        json = null;
        if (string.IsNullOrWhiteSpace(signed)) return false;

        var parts = signed.Split('.');
        if (parts.Length != 2) return false;

        var payload = parts[0];
        var signature = parts[1];

        var jsonBytes = EncodingUtils.Base64UrlDecode(payload);

        using var hmac = new HMACSHA256(_secretBytes);
        var expectedHash = hmac.ComputeHash(jsonBytes);

        var expectedSignature = EncodingUtils.Base64UrlEncode(expectedHash);

        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature)
        );

        if (valid)
            json = Encoding.UTF8.GetString(jsonBytes);

        return valid;
    }

    public bool VerifyJson<T>(string signed, out T? obj)
    {
        obj = default;
        var valid = VerifyJson(signed, out var jsonString);

        if (valid && jsonString != null)
            obj = JsonSerializer.Deserialize<T>(jsonString, _options);

        return valid;
    }
}