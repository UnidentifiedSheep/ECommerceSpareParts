using System.Text.Json.Serialization;

namespace Gateway.Application.Dtos;

public record ServiceJobsDto
{
    [JsonPropertyName("serviceName")]
    public required string ServiceName { get; init; }

    [JsonPropertyName("available")]
    public required bool Available { get; init; }

    [JsonPropertyName("jobs")]
    public required IReadOnlyList<GatewayJobInfoDto> Jobs { get; init; }
}