using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gateway.Options;
using Microsoft.Extensions.Options;

namespace Gateway.Services.Jobs;

public interface IJobAggregationService
{
    Task<GetGatewayJobsResponse> GetJobsAsync(
        HttpContext context,
        CancellationToken cancellationToken);
}//TODO: part of logic in this service must go to client

public sealed class JobAggregationService(
    IHttpClientFactory httpClientFactory,
    IOptions<JobAggregationOptions> options,
    IConfiguration configuration,
    ILogger<JobAggregationService> logger) : IJobAggregationService
{
    private const string ClientName = "jobs-aggregation";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] HeadersToForward =
    [
        "Authorization",
        "Accept-Language",
        "X-Correlation-Id",
        "traceparent",
        "tracestate"
    ];

    public async Task<GetGatewayJobsResponse> GetJobsAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var opt = options.Value;
        var client = httpClientFactory.CreateClient(ClientName);

        var tasks = opt.Services
            .Select(service => GetServiceJobsAsync(client, service, opt.RequestTimeout, context, cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return new GetGatewayJobsResponse
        {
            Services = results.ToDictionary(x => x.ServiceName, x => x.Result, StringComparer.OrdinalIgnoreCase)
        };
    }

    private async Task<ServiceJobsResponse> GetServiceJobsAsync(
        HttpClient client,
        JobServiceOptions service,
        TimeSpan timeout,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            using var request = new HttpRequestMessage(HttpMethod.Get, service.Url);
            AddForwardedHeaders(request, context);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceJobsResponse.Unavailable(
                    service.Name,
                    response.StatusCode,
                    $"Service returned {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<ServiceJobsPayload>(
                JsonOptions,
                timeoutCts.Token);

            return payload is null
                ? ServiceJobsResponse.Unavailable(service.Name, response.StatusCode, "Service returned empty response.")
                : ServiceJobsResponse.Success(service.Name, payload.Jobs.Clone());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ServiceJobsResponse.Unavailable(service.Name, null, "Service request timed out.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to aggregate jobs from {ServiceName}.", service.Name);
            return ServiceJobsResponse.Unavailable(service.Name, null, "Service is unavailable.");
        }
    }

    private void AddForwardedHeaders(
        HttpRequestMessage request,
        HttpContext context)
    {
        foreach (var header in HeadersToForward)
        {
            if (context.Request.Headers.TryGetValue(header, out var value))
                request.Headers.TryAddWithoutValidation(header, value.ToArray());
        }

        var secret = configuration["HeaderSecret:Key"];

        if (!string.IsNullOrWhiteSpace(secret))
            request.Headers.TryAddWithoutValidation("X-Internal-Token", secret);
    }
}

public sealed record GetGatewayJobsResponse
{
    [JsonPropertyName("services")]
    public required IReadOnlyDictionary<string, ServiceJobsResult> Services { get; init; }
}

public sealed record ServiceJobsResult
{
    [JsonPropertyName("available")]
    public required bool Available { get; init; }

    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; init; }

    [JsonPropertyName("jobs")]
    public JsonElement Jobs { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

internal readonly record struct ServiceJobsResponse(
    string ServiceName,
    ServiceJobsResult Result)
{
    public static ServiceJobsResponse Success(
        string service,
        JsonElement jobs) =>
        new(
            service,
            new ServiceJobsResult
            {
                Available = true,
                StatusCode = (int)HttpStatusCode.OK,
                Jobs = jobs
            });

    public static ServiceJobsResponse Unavailable(
        string service,
        HttpStatusCode? statusCode,
        string error) =>
        new(
            service,
            new ServiceJobsResult
            {
                Available = false,
                StatusCode = statusCode is null ? null : (int)statusCode,
                Jobs = JsonSerializer.SerializeToElement(Array.Empty<object>()),
                Error = error
            });
}

internal sealed record ServiceJobsPayload
{
    [JsonPropertyName("jobs")]
    public JsonElement Jobs { get; init; }
}
