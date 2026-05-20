using System.Text.Json.Serialization;
using Enums;

namespace Internal.Integration.Core.Models.Main;

public record InternalFullProduct
{
    [JsonPropertyName("product")]
    public required InternalProduct Product { get; init; }

    [JsonPropertyName("productWeight")]
    public InternalProductWeight? ProductWeight { get; init; }

    [JsonPropertyName("productSize")]
    public InternalProductSize? ProductSize { get; init; }
}

public record InternalProduct
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
}

public record InternalProductWeight
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("weight")]
    public required decimal Weight { get; init; }

    [JsonPropertyName("unit")]
    [JsonConverter(typeof(JsonStringEnumConverter<WeightUnit>))]
    public required WeightUnit Unit { get; init; }
}

public record InternalProductSize
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("length")]
    public required decimal Length { get; init; }

    [JsonPropertyName("width")]
    public required decimal Width { get; init; }

    [JsonPropertyName("height")]
    public required decimal Height { get; init; }

    [JsonPropertyName("unit")]
    [JsonConverter(typeof(JsonStringEnumConverter<DimensionUnit>))]
    public required DimensionUnit Unit { get; init; }

    [JsonPropertyName("volumeM3")]
    public required decimal VolumeM3 { get; init; }
}
