using System.Text.Json.Serialization;
using Main.Application.Dtos.Organizations;
using Main.Enums;

namespace Main.Application.Dtos.Product.Reservation;

public record ProductReservationDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("organization")]
    public required OrganizationDto Organization { get; init; }

    [JsonPropertyName("reservedCount")]
    public required int ReservedCount { get; init; }

    [JsonPropertyName("currentCount")]
    public required int CurrentCount { get; init; }

    [JsonPropertyName("proposedPrice")]
    public required decimal? ProposedPrice { get; init; }

    [JsonPropertyName("proposedCurrencyId")]
    public required int? ProposedCurrencyId { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ProductReservationStatus Status { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }

    [JsonPropertyName("whoUpdated")]
    public required Guid? WhoUpdated { get; init; }
}
