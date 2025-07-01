namespace MonoliteUnicorn.Dtos.Amw.ArticleReservations;

public class ArticleReservationDto
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
}