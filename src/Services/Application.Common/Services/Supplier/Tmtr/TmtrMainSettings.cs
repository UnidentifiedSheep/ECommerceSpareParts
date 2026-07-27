using System.Text.Json.Serialization;

namespace Application.Common.Services.Supplier.Tmtr;

internal sealed record TmtrMainSettings
{
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; }

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; init; }

    [JsonPropertyName("guaranteedDeliveryOffsetDays")]
    public int GuaranteedDeliveryOffsetDays { get; init; }

    [JsonPropertyName("authData")]
    public TmtrMainAuthData? AuthData { get; init; }
}

internal sealed record TmtrMainAuthData
{
    [JsonPropertyName("login")]
    public string? Login { get; init; }

    [JsonPropertyName("encryptedPassword")]
    public string? EncryptedPassword { get; init; }
}
