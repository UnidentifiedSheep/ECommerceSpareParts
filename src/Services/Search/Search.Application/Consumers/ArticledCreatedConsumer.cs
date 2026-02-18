using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;
using Search.Entities;

namespace Search.Application.Consumers;

public class ArticledCreatedConsumer(IArticleService articleService) : IConsumer<ArticlesCreatedEvent>
{
    public Task Consume(ConsumeContext<ArticlesCreatedEvent> context)
    {
        List<Article> adapted = context.Message.Articles.ToArticles();
        articleService.AddRange(adapted);
        return Task.CompletedTask;
    }
}