using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Users;

public record UserDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("userName")]
    public required string UserName { get; init; }
    
    [JsonPropertyName("normalizedUserName")]
    public required string NormalizedUserName { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }
    
    [JsonPropertyName("twoFactorEnabled")]
    public required bool TwoFactorEnabled { get; init; }
    
    [JsonPropertyName("lockoutEnd")]
    public required DateTime? LockoutEnd { get; init; }
    
    [JsonPropertyName("accessFailedCount")]
    public required int AccessFailedCount { get; init; }
    
    [JsonPropertyName("lastLoginAt")]
    public required DateTime? LastLoginAt { get; init; }
    
    [JsonPropertyName("userInfo")]
    public required UserInfoDto? UserInfo { get; init; }
}