using System.Text.Json.Serialization;
using Abstractions.Interfaces;

namespace Contracts.Job;

public record JobStatusUpdatedEvent : IKeyedEvent
{
    [JsonPropertyName("jobId")]
    public required Guid JobId { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("attempts")]
    public required int CurrentAttempt { get; init; }

    public string GetKey() { return $"job-status-updated:{JobId}"; }
}