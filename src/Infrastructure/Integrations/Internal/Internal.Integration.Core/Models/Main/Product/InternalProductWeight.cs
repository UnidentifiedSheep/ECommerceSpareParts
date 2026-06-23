using System.Text.Json.Serialization;
using Enums;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalProductWeight
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("weight")]
    public required decimal Weight { get; init; }

    [JsonPropertyName("unit")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required WeightUnit Unit { get; init; }
}
