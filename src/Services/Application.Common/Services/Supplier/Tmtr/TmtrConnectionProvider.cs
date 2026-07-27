using Abstractions.Interfaces.Services;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Enums;
using Integrations.Supplier.Interfaces;

namespace Application.Common.Services.Supplier.Tmtr;

public class TmtrConnectionProvider(
    TmtrMainSettingProvider settingsProvider,
    ISecretEncryptor secretEncryptor
) : IConnectionProvider<TmtrConnection>
{
    public async Task<TmtrConnection> GetConnectionAsync(
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

    public async Task<ConnectionCheck<TmtrConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await settingsProvider.GetAsync(cancellationToken);
        if (!result.IsSuccess)
            return Unavailable(
                result.Reason ?? SupplierUnavailableReason.SettingsUnavailable,
                result.Message ?? "Unable to get TMTR settings");

        var settings = result.Setting!;
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
