using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Common;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Common;

internal sealed class JobNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) : InternalClientBase(authClient, optionsMonitor)
{
    public async Task<IReadOnlyList<InternalJobInfo>> GetAvailableJobs(
        ServiceOptions options,
        string? locale,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(new Uri(options.Url), "/jobs/available");
        
        using var request = await GetRequest(
            HttpMethod.Get,
            url.ToString(),
            cancellationToken);
        
        AddLocalizationHeader(request, locale);
        
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetAvailableJobsResponse>(json)?.Jobs
               ?? throw new InvalidOperationException($"{nameof(GetAvailableJobs)} returned null.");
    }

    private record GetAvailableJobsResponse
    {
        [JsonPropertyName("jobs")]
        public required IReadOnlyList<InternalJobInfo> Jobs { get; init; }
    }
}