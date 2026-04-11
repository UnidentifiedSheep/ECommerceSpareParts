using Domain;

namespace Main.Entities;

public class StorageContentReservation : AuditableEntity<StorageContentReservation, int>
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int ProductId { get; set; }

    public int InitialCount { get; set; }

    public int CurrentCount { get; set; }

    public decimal? GivenPrice { get; set; }

    public int? GivenCurrencyId { get; set; }

    public bool IsDone { get; set; }

    public string? Comment { get; set; }

    public Guid WhoCreated { get; set; }

    public Guid? WhoUpdated { get; set; }
    public override int GetId() => Id;
}