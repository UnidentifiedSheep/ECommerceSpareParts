using System.Text.Json.Serialization;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Storage;

public record StorageDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string? Description { get; init; }

    [JsonPropertyName("location")]
    public required string? Location { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<StorageType>))]
    public required StorageType Type { get; init; }
}