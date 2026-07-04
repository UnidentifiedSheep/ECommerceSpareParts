using System.Text.Json.Serialization;
using Domain.CommonEnums;

namespace Application.Common.Dtos;

public record JobDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required JobStatus Status { get; init; }

    [JsonPropertyName("attempts")]
    public required int Attempts { get; init; }

    [JsonPropertyName("maxAttempts")]
    public required int MaxAttempts { get; init; }

    [JsonPropertyName("errorMessage")]
    public required string? ErrorMessage { get; init; }

    [JsonPropertyName("lockedAt")]
    public required DateTime? LockedAt { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }

    [JsonPropertyName("createdBy")]
    public required Guid? CreatedBy { get; init; }
}