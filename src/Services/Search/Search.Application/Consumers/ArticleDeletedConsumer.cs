using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Consumers;

public class ArticleDeletedConsumer(IArticleWriteService articleWriteService) : IConsumer<ArticleDeletedEvent>
{
    public Task Consume(ConsumeContext<ArticleDeletedEvent> context)
    {
        articleWriteService.Delete(context.Message.Id);
        return Task.CompletedTask;
    }
}