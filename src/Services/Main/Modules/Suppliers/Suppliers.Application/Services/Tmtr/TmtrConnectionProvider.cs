using Integrations.Common;
using Application.Common.Interfaces.Settings;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Main.Entities.Suppliers.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.Services.Tmtr;

public class TmtrConnectionProvider(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor) : IConnectionProvider<TmtrConnection>
{
	public async Task<TmtrConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
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

	public async Task<ConnectionCheck<TmtrConnection>> CheckConnectionAsync(
		CancellationToken cancellationToken = default)
	{
		var settings = (await settingsService.GetOrDefault<TmtrSupplierSetting>(cancellationToken)).Data;
		if (!settings.IsEnabled)
			return Unavailable(ConnectionUnavailableReason.Disabled, "TMTR integration is disabled");

		if (string.IsNullOrWhiteSpace(settings.BaseUrl))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "TMTR BaseUrl is empty");

		if (string.IsNullOrWhiteSpace(settings.AuthData?.Login))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "TMTR login is empty");

		if (string.IsNullOrWhiteSpace(settings.AuthData.EncryptedPassword))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "TMTR password is empty");

		if (!secretEncryptor.TryDecrypt(settings.AuthData.EncryptedPassword, out var password))
			return Unavailable(
				ConnectionUnavailableReason.InvalidConfiguration,
				"TMTR password cannot be decrypted");

		return new ConnectionCheck<TmtrConnection>(
			true,
			new TmtrConnection
			{
				BaseUrl = settings.BaseUrl,
				Login = settings.AuthData.Login,
				Password = password!
			});
	}

	private static ConnectionCheck<TmtrConnection> Unavailable(
		ConnectionUnavailableReason reason,
		string message)
		=> new(
			false,
			null,
			reason,
			message);
}
