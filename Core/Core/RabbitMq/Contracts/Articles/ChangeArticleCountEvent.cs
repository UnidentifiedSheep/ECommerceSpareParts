namespace Core.RabbitMq.Contracts.Articles;

public record ChangeArticleCountEvent(Dictionary<int, int> ArticleCounts);