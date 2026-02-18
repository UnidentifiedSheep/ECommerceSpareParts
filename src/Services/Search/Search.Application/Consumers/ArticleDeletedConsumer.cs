using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Consumers;

public class ArticleDeletedConsumer(IArticleService articleService) : IConsumer<ArticleDeletedEvent>
{
    public Task Consume(ConsumeContext<ArticleDeletedEvent> context)
    {
        articleService.Delete(context.Message.Id);
        return Task.CompletedTask;
    }
}