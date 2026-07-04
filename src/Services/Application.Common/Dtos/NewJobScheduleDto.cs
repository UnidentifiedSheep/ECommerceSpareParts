using System.Text.Json.Serialization;

namespace Application.Common.Dtos;

public record NewJobScheduleDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("jobSystemName")]
    public required string JobSystemName { get; init; }

    [JsonPropertyName("inputState")]
    public required string InputState { get; init; }

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts { get; init; } = 3;

    [JsonPropertyName("cron")]
    public required string Cron { get; init; }

    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}