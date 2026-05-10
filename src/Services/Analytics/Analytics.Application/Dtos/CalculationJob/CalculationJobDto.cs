using System.Text.Json.Serialization;
using Analytics.Enums;

namespace Analytics.Application.Dtos.CalculationJob;

public record CalculationJobDto
{
    [JsonPropertyName("requestId")]
    public required Guid RequestId { get; init; }

    [JsonPropertyName("metricId")]
    public Guid? MetricId { get; init; }

    [JsonPropertyName("metricSystemName")]
    public required string MetricSystemName { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter<CalculationStatus>))]
    public required CalculationStatus Status { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreateAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public required DateTime UpdateAt { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }
}