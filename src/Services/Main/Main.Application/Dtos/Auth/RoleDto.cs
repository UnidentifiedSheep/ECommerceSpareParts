using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Roles;

public record RoleDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string? Description { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }
    
    [JsonPropertyName("whoCreated")]
    public required Guid WhoCreated { get; init; }
    
    [JsonPropertyName("whoUpdated")]
    public required Guid? WhoUpdated { get; init; }
}