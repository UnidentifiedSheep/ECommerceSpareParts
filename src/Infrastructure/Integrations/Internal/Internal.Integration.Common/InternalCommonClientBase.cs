using Abstractions.Interfaces;
using Internal.Integration.Core;
using Internal.Integration.Core.Extensions;
using Internal.Integration.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

public abstract class InternalCommonClientBase(
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor,
    IOptionsMonitor<InternalServicesOptions> serviceOptions
    ) : InternalClientBase(authClient, optionsMonitor)
{
    protected async Task<HttpRequestMessage> GetRequest(
        IServiceDefinition serviceDefinition,
        HttpMethod method,
        string url,
        CancellationToken cancellationToken = default)
    {
        var options = serviceOptions.CurrentValue.GetOptionsForService(serviceDefinition);
        
        return await GetRequest(
            method,
            new Uri(new Uri(options.Url), url).ToString(),
            cancellationToken);
    }
}