using Contracts.Articles;
using MassTransit;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;

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