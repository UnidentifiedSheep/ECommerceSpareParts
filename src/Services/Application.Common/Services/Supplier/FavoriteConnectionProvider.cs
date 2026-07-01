using System.Text.Json.Serialization;
using Abstractions;
using Abstractions.Interfaces.Services;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Enums;
using Integrations.Supplier.Interfaces;
using Internal.Integration.Core.Interfaces.Common;

namespace Application.Common.Services.Supplier;

public class FavoriteConnectionProvider(
    ICommonClient commonClient,
    ISecretEncryptor encryptor) : IConnectionProvider<FavoritConnection>
{
    private const string SettingSystemName = "FavoritSupplierSetting";
    public virtual async Task<FavoritConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var check = await CheckConnectionAsync(cancellationToken);
        if (!check.CanUse || check.Connection is null)
            throw new InvalidOperationException(
                $"Supplier cannot be user. Reason: {check.Reason.ToString()}. Message: {check.Message}");
        
        return check.Connection;
    }

    public virtual async Task<ConnectionCheck<FavoritConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await commonClient.SettingNode.GetSetting(
            serviceDefinition: ServicesDefinitions.Main,
            systemName: SettingSystemName,
            cancellationToken: cancellationToken);

        if (!response.Success)
        {
            return new ConnectionCheck<FavoritConnection>(
                CanUse: false,
                Connection: null,
                Reason: SupplierUnavailableReason.SettingsUnavailable,
                Message: "Unable to get Favorit settings");
        }

        var settings = System.Text.Json.JsonSerializer.Deserialize<FavoritMainSettings>(
            response.ValueOrThrow);

        if (settings is null)
            return new ConnectionCheck<FavoritConnection>(
                CanUse: false,
                Connection: null,
                Reason: SupplierUnavailableReason.InvalidConfiguration,
                Message: "Invalid Favorit settings JSON");

        if (!settings.IsEnabled)
            return new ConnectionCheck<FavoritConnection>(
                CanUse: false,
                Connection: null,
                Reason: SupplierUnavailableReason.Disabled,
                Message: "Favorit integration is disabled");

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            return new ConnectionCheck<FavoritConnection>(
                CanUse: false,
                Connection: null,
                Reason: SupplierUnavailableReason.InvalidConfiguration,
                Message: "Favorit BaseUrl is empty");

        if (string.IsNullOrWhiteSpace(settings.EncryptedApiKey))
            return new ConnectionCheck<FavoritConnection>(
                CanUse: false,
                Connection: null,
                Reason: SupplierUnavailableReason.InvalidConfiguration,
                Message: "Favorit ApiKey is empty");

        return new ConnectionCheck<FavoritConnection>(
            CanUse: true,
            Connection: new FavoritConnection
            {
                BaseUrl = settings.BaseUrl,
                ApiKey = encryptor.Decrypt(settings.EncryptedApiKey)
            });
    }

    protected record FavoritMainSettings
    {
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; init; }
    
        [JsonPropertyName("baseUrl")]
        public string? BaseUrl { get; init; }
    
        [JsonPropertyName("encryptedApiKey")]
        public string? EncryptedApiKey { get; init; }
    }
}