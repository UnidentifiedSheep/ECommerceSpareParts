using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Consumers;

public class ArticleDeletedConsumer(IArticleWriteService articleWriteService) : IConsumer<ProductDeletedEvent>
{
    public Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        articleWriteService.Delete(context.Message.Id);
        return Task.CompletedTask;
    }
}