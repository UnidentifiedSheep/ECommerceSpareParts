using System.Text.Json.Serialization;
using Application.Common.Static;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier.Tmtr;

public sealed class TmtrMainSettingProvider(IFusionCache cache, ISupplierSettingSource settingSource)
	: MainSupplierSettingProvider<TmtrMainSettings>(
		cache,
		settingSource,
		"TmtrSupplierSetting",
		CacheKeys.SettingsCache.TmtrSettings);

public sealed record TmtrMainSettings
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

public sealed record TmtrMainAuthData
{
	[JsonPropertyName("login")]
	public string? Login { get; init; }

	[JsonPropertyName("encryptedPassword")]
	public string? EncryptedPassword { get; init; }
}
