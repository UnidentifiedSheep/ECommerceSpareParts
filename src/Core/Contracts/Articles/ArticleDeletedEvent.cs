namespace Contracts.Articles;

public record ArticleDeletedEvent
{
    public int Id { get; init; }
}