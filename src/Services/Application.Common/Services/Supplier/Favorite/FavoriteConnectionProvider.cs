using Abstractions.Interfaces.Services;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Enums;
using Integrations.Supplier.Interfaces;

namespace Application.Common.Services.Supplier.Favorite;

public class FavoriteConnectionProvider(
    FavoriteMainSettingProvider settingsProvider,
    ISecretEncryptor secretEncryptor
) : IConnectionProvider<FavoritConnection>
{
    public async Task<FavoritConnection> GetConnectionAsync(
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

    public async Task<ConnectionCheck<FavoritConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await settingsProvider.GetAsync(cancellationToken);
        if (!result.IsSuccess)
            return Unavailable(
                result.Reason ?? SupplierUnavailableReason.SettingsUnavailable,
                result.Message ?? "Unable to get Favorit settings");

        var settings = result.Setting!;
        if (!settings.IsEnabled)
            return Unavailable(
                SupplierUnavailableReason.Disabled,
                "Favorit integration is disabled");

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "Favorit BaseUrl is empty");

        if (string.IsNullOrWhiteSpace(settings.EncryptedApiKey))
            return Unavailable(
                SupplierUnavailableReason.InvalidConfiguration,
                "Favorit ApiKey is empty");

        return new ConnectionCheck<FavoritConnection>(
            true,
            new FavoritConnection
            {
                BaseUrl = settings.BaseUrl,
                ApiKey = secretEncryptor.Decrypt(settings.EncryptedApiKey)
            });
    }

    private static ConnectionCheck<FavoritConnection> Unavailable(
        SupplierUnavailableReason reason,
        string message)
    {
        return new ConnectionCheck<FavoritConnection>(
            false,
            null,
            reason,
            message);
    }
}
