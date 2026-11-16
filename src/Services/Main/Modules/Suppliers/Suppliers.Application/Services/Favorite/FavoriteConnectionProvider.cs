using Application.Common.Interfaces.Settings;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Enums;
using Integrations.Supplier.Interfaces;
using Main.Entities.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.Services.Favorite;

public class FavoriteConnectionProvider(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor) : IConnectionProvider<FavoritConnection>
{
	public async Task<FavoritConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		var check = await CheckConnectionAsync(cancellationToken);
		if (!check.CanUse || check.Connection is null)
			throw new InvalidOperationException(
				$"Supplier cannot be used. Reason: {check.Reason}. Message: {check.Message}");

		return check.Connection;
	}

	async Task<ConnectionCheck> IConnectionProvider.
		CheckConnectionAsync(CancellationToken cancellationToken)
		=> await CheckConnectionAsync(cancellationToken);

	public async Task<ConnectionCheck<FavoritConnection>> CheckConnectionAsync(
		CancellationToken cancellationToken = default)
	{
		var settings = (await settingsService.GetOrDefault<FavoritSupplierSetting>(cancellationToken)).Data;
		if (!settings.IsEnabled)
			return Unavailable(SupplierUnavailableReason.Disabled, "Favorit integration is disabled");

		if (string.IsNullOrWhiteSpace(settings.BaseUrl))
			return Unavailable(SupplierUnavailableReason.InvalidConfiguration, "Favorit BaseUrl is empty");

		if (string.IsNullOrWhiteSpace(settings.EncryptedApiKey))
			return Unavailable(SupplierUnavailableReason.InvalidConfiguration, "Favorit ApiKey is empty");

		if (!secretEncryptor.TryDecrypt(settings.EncryptedApiKey, out var apiKey))
			return Unavailable(
				SupplierUnavailableReason.InvalidConfiguration,
				"Favorit ApiKey cannot be decrypted");

		return new ConnectionCheck<FavoritConnection>(
			true,
			new FavoritConnection
			{
				BaseUrl = settings.BaseUrl, ApiKey = apiKey!
			});
	}

	private static ConnectionCheck<FavoritConnection> Unavailable(
		SupplierUnavailableReason reason,
		string message)
		=> new(
			false,
			null,
			reason,
			message);
}
