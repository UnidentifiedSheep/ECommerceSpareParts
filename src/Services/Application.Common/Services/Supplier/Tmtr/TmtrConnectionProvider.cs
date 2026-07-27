using Abstractions;
using Abstractions.Interfaces.Services;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Enums;
using Integrations.Supplier.Interfaces;
using Internal.Integration.Core.Interfaces.Common;

namespace Application.Common.Services.Supplier.Tmtr;

public class TmtrConnectionProvider(
    ICommonClient commonClient,
    ISecretEncryptor secretEncryptor
) : IConnectionProvider<TmtrConnection>
{
    private const string SettingSystemName = "TmtrSupplierSetting";

    public virtual async Task<TmtrConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var check = await CheckConnectionAsync(cancellationToken);
        if (!check.CanUse || check.Connection is null)
            throw new InvalidOperationException(
                $"Supplier cannot be used. Reason: {check.Reason}. Message: {check.Message}");

        return check.Connection;
    }

    async Task<ConnectionCheck> IConnectionProvider.CheckConnectionAsync(
        CancellationToken cancellationToken)
    {
        return await CheckConnectionAsync(cancellationToken);
    }

    public virtual async Task<ConnectionCheck<TmtrConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await commonClient.SettingNode.GetSetting(
            ServicesDefinitions.Main,
            SettingSystemName,
            cancellationToken);

        if (!response.Success)
            return Unavailable(
                SupplierUnavailableReason.SettingsUnavailable,
                "Unable to get TMTR settings");

        var settings = System.Text.Json.JsonSerializer.Deserialize<TmtrMainSettings>(
            response.ValueOrThrow);

        if (settings is null)
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "Invalid TMTR settings JSON");

        if (!settings.IsEnabled)
            return Unavailable(
                SupplierUnavailableReason.Disabled,
                "TMTR integration is disabled");

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "TMTR BaseUrl is empty");

        if (string.IsNullOrWhiteSpace(settings.AuthData?.Login))
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "TMTR login is empty");

        if (string.IsNullOrWhiteSpace(settings.AuthData.EncryptedPassword))
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "TMTR password is empty");

        return new ConnectionCheck<TmtrConnection>(
            true,
            new TmtrConnection
            {
                BaseUrl = settings.BaseUrl,
                Login = settings.AuthData.Login,
                Password = secretEncryptor.Decrypt(settings.AuthData.EncryptedPassword)
            });
    }

    private static ConnectionCheck<TmtrConnection> Unavailable(
        SupplierUnavailableReason reason,
        string message)
    {
        return new ConnectionCheck<TmtrConnection>(
            false,
            null,
            reason,
            message);
    }
}
