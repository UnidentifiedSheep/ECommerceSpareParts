using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;

namespace Search.Application.Consumers;

public class ArticledCreatedConsumer(IArticleWriteService articleWriteService) : IConsumer<ProductCreatedEvent>
{
    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var adapted = context.Message.Articles.ToArticles();
        articleWriteService.AddRange(adapted);
        return Task.CompletedTask;
    }
}