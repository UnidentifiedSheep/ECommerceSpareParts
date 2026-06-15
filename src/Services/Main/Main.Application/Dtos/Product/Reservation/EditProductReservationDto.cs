using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product.Reservation;

public record EditProductReservationDto
{
    [JsonPropertyName("proposedPrice")]
    public required decimal? GivenPrice { get; init; }

    [JsonPropertyName("givenCurrencyId")]
    public required int? GivenCurrencyId { get; init; }

    [JsonPropertyName("comment")]
    public required string? Comment { get; init; }
}
