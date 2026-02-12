namespace Contracts.Articles;

public record ArticleBuyPricesChangedEvent
{
    public IEnumerable<int> ArticleIds { get; init; } = null!;

}