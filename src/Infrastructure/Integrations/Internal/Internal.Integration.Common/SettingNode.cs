using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Common;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

public class SettingNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServicesOptions> serviceOptions,
    IOptionsMonitor<InternalServiceCredentials> credentialsMonitor
    ) : InternalCommonClientBase(authClient, credentialsMonitor, serviceOptions), ISettingNode
{
    public async Task<Response<string>> GetSetting(
        IServiceDefinition serviceDefinition, 
        string systemName, 
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            serviceDefinition,
            HttpMethod.Get,
            "/internal/settings/" + systemName,
            cancellationToken);
        
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<GetSettingResponse, string>(
            response, 
            x => x.Json,
            cancellationToken);
    }
    
    private record GetSettingResponse
    {
        [JsonPropertyName("json")]
        public required string Json { get; init; }
    }
}