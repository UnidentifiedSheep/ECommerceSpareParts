using System.Text.Json.Serialization;

namespace Application.Common.Dtos;

public record JobScheduleDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }

    [JsonPropertyName("jobSystemName")]
    public required string JobSystemName { get; init; }

    [JsonPropertyName("inputState")]
    public required string InputState { get; init; }

    [JsonPropertyName("maxAttempts")]
    public required int MaxAttempts { get; init; }

    [JsonPropertyName("cron")]
    public required string Cron { get; init; }

    [JsonPropertyName("localizedCron")]
    public required string LocalizedCron { get; init; }

    [JsonPropertyName("lastQueuedAt")]
    public required DateTime? LastQueuedAt { get; init; }

    [JsonPropertyName("nextRunAt")]
    public required DateTime? NextRunAt { get; init; }

    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}