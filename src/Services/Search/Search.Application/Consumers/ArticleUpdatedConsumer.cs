using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;

namespace Search.Application.Consumers;

public class ArticleUpdatedConsumer(IArticleWriteService articleWriteService) : IConsumer<ProductUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var article = context.Message.Article.ToArticle();
        articleWriteService.Add(article);
        return Task.CompletedTask;
    }
}