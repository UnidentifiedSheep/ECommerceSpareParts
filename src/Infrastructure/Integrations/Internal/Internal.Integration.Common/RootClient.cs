using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Common;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

public class RootClient(
    IOptionsMonitor<InternalServicesOptions> serviceOptions,
    IOptionsMonitor<InternalServiceCredentials> credentialsMonitor,
    IAuthClient authClient,
    HttpClient httpClient) : ICommonClient
{
    public IJobNode JobNode { get; } 
        = new JobNode(httpClient, authClient, serviceOptions, credentialsMonitor);

    public ISettingNode SettingNode { get; } 
        = new SettingNode(httpClient, authClient, serviceOptions, credentialsMonitor);
}