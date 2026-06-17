using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Common;

public class InternalJobInfo
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string Description { get; init; }
    
    [JsonPropertyName("initStateSchema")]
    public required string InitStateSchema { get; init; }
}