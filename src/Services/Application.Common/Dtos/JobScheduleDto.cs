using System.Text.Json.Serialization;

namespace Application.Common.Dtos;

public record JobScheduleDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("inputState")]
    public required string InputState { get; init; }
    
    [JsonPropertyName("maxAttempts")]
    public required int MaxAttempts { get; init; }
    
    [JsonPropertyName("cron")]
    public required string Cron { get; init; }
    
    [JsonPropertyName("lastQueuedAt")]
    public required DateTime? LastQueuedAt { get; init; }
    
    [JsonPropertyName("nextRunAt")]
    public required DateTime? NextRunAt { get; init; }
}