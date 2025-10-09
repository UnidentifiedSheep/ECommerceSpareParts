namespace Core.Entities;

public partial class StorageContentReservation
{
    public Guid UserId { get; set; }

    public int ArticleId { get; set; }

    public int InitialCount { get; set; }

    public int CurrentCount { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? GivenPrice { get; set; }

    public int? GivenCurrencyId { get; set; }

    public bool IsDone { get; set; }

    public string? Comment { get; set; }

    public int Id { get; set; }

    public Guid WhoCreated { get; set; }

    public Guid? WhoUpdated { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Currency? GivenCurrency { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual User WhoCreatedNavigation { get; set; } = null!;

    public virtual User? WhoUpdatedNavigation { get; set; }
}
