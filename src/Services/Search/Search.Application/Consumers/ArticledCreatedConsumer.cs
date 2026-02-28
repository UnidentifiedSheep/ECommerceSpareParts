using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;
using Search.Entities;

namespace Search.Application.Consumers;

public class ArticledCreatedConsumer(IArticleWriteService articleWriteService) : IConsumer<ArticlesCreatedEvent>
{
    public Task Consume(ConsumeContext<ArticlesCreatedEvent> context)
    {
        List<Article> adapted = context.Message.Articles.ToArticles();
        articleWriteService.AddRange(adapted);
        return Task.CompletedTask;
    }
}