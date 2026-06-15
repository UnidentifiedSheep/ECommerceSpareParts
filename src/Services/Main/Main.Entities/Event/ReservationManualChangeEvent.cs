using System.Text.Json.Serialization;
using Main.Entities.Storage;

namespace Main.Entities.Event;

public class ReservationManualChangeEvent : Event<ReservationManualChangeEventData>
{
    public int ReservationId { get; private set; }
    public ReservationManualChangeEvent(
        int reservationId,
        ReservationManualChangeEventData data) : base(data)
    {
        ReservationId = reservationId;
    }

    private ReservationManualChangeEvent()
    {
    }

    public static ReservationManualChangeEvent Create(
        int reservationId,
        ReservationManualChangeEventData data)
    {
        return new ReservationManualChangeEvent(reservationId, data);
    }

    public static ReservationManualChangeEvent Create(StorageContentReservation reservation)
    {
        var data = new ReservationManualChangeEventData
        {
            Comment = reservation.Comment,
            ProposedCurrencyId = reservation.ProposedCurrencyId,
            ProposePrice = reservation.ProposedPrice,
            UpdatedAt = reservation.UpdatedAt,
            UpdatedBy = reservation.WhoUpdated
        };

        return new ReservationManualChangeEvent(reservation.Id, data);
    }
}

public record ReservationManualChangeEventData
{
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
    
    [JsonPropertyName("proposePrice")]
    public decimal? ProposePrice { get; init; }
    
    [JsonPropertyName("proposedCurrencyId")]
    public int? ProposedCurrencyId { get; init; }
    
    [JsonPropertyName("updatedBy")]
    public Guid? UpdatedBy { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; init; }
}