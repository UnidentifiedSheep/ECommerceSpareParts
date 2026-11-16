using Application.Common.Interfaces.Settings;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Main.Entities.Suppliers.Settings.Supplier;
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
			return Unavailable(ConnectionUnavailableReason.Disabled, "Favorit integration is disabled");

		if (string.IsNullOrWhiteSpace(settings.BaseUrl))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Favorit BaseUrl is empty");

		if (string.IsNullOrWhiteSpace(settings.EncryptedApiKey))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Favorit ApiKey is empty");

		if (!secretEncryptor.TryDecrypt(settings.EncryptedApiKey, out var apiKey))
			return Unavailable(
				ConnectionUnavailableReason.InvalidConfiguration,
				"Favorit ApiKey cannot be decrypted");

		return new ConnectionCheck<FavoritConnection>(
			true,
			new FavoritConnection
			{
				BaseUrl = settings.BaseUrl, ApiKey = apiKey!
			});
	}

	private static ConnectionCheck<FavoritConnection> Unavailable(
		ConnectionUnavailableReason reason,
		string message)
		=> new(
			false,
			null,
			reason,
			message);
}
