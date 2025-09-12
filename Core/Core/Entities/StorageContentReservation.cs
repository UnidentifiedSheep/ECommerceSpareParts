namespace Core.Entities;

public class StorageContentReservation
{
    public string UserId { get; set; } = null!;

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

    public string WhoCreated { get; set; } = null!;

    public string? WhoUpdated { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Currency? GivenCurrency { get; set; }

    public virtual AspNetUser User { get; set; } = null!;

    public virtual AspNetUser WhoCreatedNavigation { get; set; } = null!;

    public virtual AspNetUser? WhoUpdatedNavigation { get; set; }
}