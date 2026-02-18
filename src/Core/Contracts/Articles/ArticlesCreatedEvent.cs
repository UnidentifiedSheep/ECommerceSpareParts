using Contracts.Models.Articles;

namespace Contracts.Articles;

public record ArticlesCreatedEvent
{
    public List<Article> Articles { get; init; } = [];
}