using System.Text.Json.Serialization;
using Application.Common.Static;
using Internal.Integration.Core.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier.Favorite;

public sealed class FavoriteMainSettingProvider(IFusionCache cache, ICommonClient commonClient)
	: MainSupplierSettingProvider<FavoriteMainSettings>(
		cache,
		commonClient,
		"FavoritSupplierSetting",
		CacheKeys.SettingsCache.FavoritSettings);

public sealed record FavoriteMainSettings
{
	[JsonPropertyName("isEnabled")]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("encryptedApiKey")]
	public string? EncryptedApiKey { get; init; }
}
