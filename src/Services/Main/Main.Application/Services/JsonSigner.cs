using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core.Attributes;

namespace Main.Application.Services;

public class JsonSigner : BaseSigner
{
    private readonly byte[] _secretBytes;

    public JsonSigner(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        _secretBytes = Encoding.UTF8.GetBytes(secret);
    }

    public string Sign<T>(T data)
    {
        ArgumentNullException.ThrowIfNull(data);

        string json = JsonSerializer.Serialize(data, Global.JsonOptions); 
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using var hmac = new HMACSHA256(_secretBytes);
        byte[] hash = hmac.ComputeHash(jsonBytes);

        string payload = Base64UrlEncode(jsonBytes);
        string signature = Base64UrlEncode(hash);

        return $"{payload}.{signature}";
    }

    public bool VerifyJson(string signed, out string? json)
    {
        json = null;
        if (string.IsNullOrWhiteSpace(signed)) return false;

        var parts = signed.Split('.');
        if (parts.Length != 2) return false;

        string payload = parts[0];
        string signature = parts[1];

        byte[] jsonBytes = Base64UrlDecode(payload);

        using var hmac = new HMACSHA256(_secretBytes);
        byte[] expectedHash = hmac.ComputeHash(jsonBytes);

        string expectedSignature = Base64UrlEncode(expectedHash);

        bool valid = CryptographicOperations.FixedTimeEquals(
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
        bool valid = VerifyJson(signed, out string? jsonString);

        if (valid && jsonString != null)
            obj = JsonSerializer.Deserialize<T>(jsonString, Global.JsonOptions);

        return valid;
    }
}