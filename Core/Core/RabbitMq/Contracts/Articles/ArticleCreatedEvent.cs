using Core.RabbitMq.Contracts.Articles.Models;

namespace Core.RabbitMq.Contracts.Articles;

public record ArticleCreatedEvent(IEnumerable<ArticleModel> Articles);