using Application.Common.Interfaces.Settings;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Main.Entities.Suppliers.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.Services.Armtek;

public class ArmtekConnectionProvider(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor) : IConnectionProvider<ArmtekConnection>
{
	public async Task<ArmtekConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
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

	public async Task<ConnectionCheck<ArmtekConnection>> CheckConnectionAsync(
		CancellationToken cancellationToken = default)
	{
		var settings = (await settingsService.GetOrDefault<ArmtekSupplierSetting>(cancellationToken)).Data;
		if (!settings.IsEnabled)
			return Unavailable(ConnectionUnavailableReason.Disabled, "Armtek integration is disabled");

		if (string.IsNullOrWhiteSpace(settings.BaseUrl))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Armtek BaseUrl is empty");

		if (string.IsNullOrWhiteSpace(settings.Login))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Armtek login is empty");

		if (string.IsNullOrWhiteSpace(settings.EncryptedPassword))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Armtek password is empty");

		if (string.IsNullOrWhiteSpace(settings.SalesOrganization))
			return Unavailable(
				ConnectionUnavailableReason.InvalidConfiguration,
				"Armtek sales organization is empty");

		if (string.IsNullOrWhiteSpace(settings.CustomerCode))
			return Unavailable(ConnectionUnavailableReason.InvalidConfiguration, "Armtek customer code is empty");

		if (!secretEncryptor.TryDecrypt(settings.EncryptedPassword, out var password))
			return Unavailable(
				ConnectionUnavailableReason.InvalidConfiguration,
				"Armtek password cannot be decrypted");

		return new ConnectionCheck<ArmtekConnection>(
			true,
			new ArmtekConnection
			{
				BaseUrl = settings.BaseUrl,
				Login = settings.Login,
				Password = password!,
				SalesOrganization = settings.SalesOrganization,
				CustomerCode = settings.CustomerCode,
				ContractCode = settings.ContractCode,
				Storages = settings
					.Storages
					.Select(x => new ArmtekStorageConnection
					{
						StorageCode = x.StorageCode,
						DeliveryMode = x.DeliveryMode,
						PointCode = x.PointCode
					})
					.ToList()
			});
	}

	private static ConnectionCheck<ArmtekConnection> Unavailable(
		ConnectionUnavailableReason reason,
		string message)
		=> new(
			false,
			null,
			reason,
			message);
}
