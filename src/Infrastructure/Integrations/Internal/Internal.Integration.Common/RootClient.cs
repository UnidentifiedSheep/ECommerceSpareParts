using Abstractions.Interfaces;
using Internal.Integration.Core;
using Internal.Integration.Core.Extensions;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Common;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

public class RootClient(
    IOptionsMonitor<InternalServicesOptions> serviceOptions,
    IOptionsMonitor<InternalServiceCredentials> credentialsMonitor,
    IAuthClient authClient,
    HttpClient httpClient) : InternalClientBase(authClient, credentialsMonitor), ICommonClient
{
    private readonly JobNode _jobNode = new(httpClient, authClient, credentialsMonitor);
    
    public async Task<IReadOnlyList<InternalJobInfo>> GetAvailableJobs(
        IServiceDefinition serviceDefinition, 
        string? locale,
        CancellationToken cancellationToken = default)
    {
        var options = serviceOptions.CurrentValue.GetOptionsForService(serviceDefinition);
        return await _jobNode.GetAvailableJobs(options, locale, cancellationToken);
    }
}