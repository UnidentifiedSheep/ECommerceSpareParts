using System.Text.Json.Serialization;
using Domain.CommonEnums;

namespace Analytics.Application.Dtos.Metric;

public record MetricJobDto
{
    [JsonPropertyName("jobId")]
    public required Guid JobId { get; init; }

    [JsonPropertyName("metricId")]
    public required Guid MetricId { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required JobStatus Status { get; init; }

    [JsonPropertyName("attempts")]
    public required int Attempts { get; init; }

    [JsonPropertyName("maxAttempts")]
    public required int MaxAttempts { get; init; }

    [JsonPropertyName("errorMessage")]
    public required string? ErrorMessage { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }
}