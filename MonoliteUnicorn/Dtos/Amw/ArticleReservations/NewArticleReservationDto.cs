namespace MonoliteUnicorn.Dtos.Amw.ArticleReservations;

public class NewArticleReservationDto
{
    public string UserId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int InitialCount { get; set; }

    public int CurrentCount { get; set; }

    public decimal? GivenPrice { get; set; }

    public int? GivenCurrencyId { get; set; }

    public string? Comment { get; set; }
}