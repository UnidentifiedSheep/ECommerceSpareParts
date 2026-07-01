using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Common;
using Internal.Integration.Core.Models.Common;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

internal sealed class JobNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServicesOptions> serviceOptions,
    IOptionsMonitor<InternalServiceCredentials> credentialsMonitor
)
    : InternalCommonClientBase(
        authClient,
        credentialsMonitor,
        serviceOptions), IJobNode
{
    public async Task<Response<IReadOnlyList<InternalJobInfo>>> GetAvailableJobs(
        IServiceDefinition serviceDefinition,
        string? locale,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            serviceDefinition,
            HttpMethod.Get,
            "/jobs/available",
            cancellationToken);

        AddLocalizationHeader(request, locale);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<GetAvailableJobsResponse, IReadOnlyList<InternalJobInfo>>(
            response,
            x => x.Jobs,
            cancellationToken);
    }

    private record GetAvailableJobsResponse
    {
        [JsonPropertyName("jobs")]
        public required IReadOnlyList<InternalJobInfo> Jobs { get; init; }
    }
}