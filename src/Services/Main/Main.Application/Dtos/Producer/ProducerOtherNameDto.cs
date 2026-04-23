using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Producer;

public record ProducerOtherNameDto
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
    
    [JsonPropertyName("otherName")]
    public required string OtherName { get; init; }
    
    [JsonPropertyName("whereUsed")]
    public required string WhereUsed { get; init; }
}