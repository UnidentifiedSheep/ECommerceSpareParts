using Contracts.Models.Articles;

namespace Contracts.Articles;

public record ArticleUpdatedEvent
{
    public Article Article { get; init; } = null!;
}