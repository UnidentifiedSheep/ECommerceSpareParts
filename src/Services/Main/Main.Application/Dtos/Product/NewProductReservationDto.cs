using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record NewProductReservationDto
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("reservedCount")]
    public required int ReservedCount { get; init; }

    [JsonPropertyName("currentCount")]
    public required int CurrentCount { get; init; }

    [JsonPropertyName("proposedPrice")]
    public required decimal? ProposedPrice { get; init; }
    
    [JsonPropertyName("givenCurrencyId")]
    public required int? GivenCurrencyId { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}