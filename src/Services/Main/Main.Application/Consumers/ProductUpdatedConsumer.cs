using Application.Common.Interfaces;
using Contracts.Articles;
using Main.Entities.Product;
using MassTransit;

namespace Main.Application.Consumers;

public class ProductUpdatedConsumer(
    ICacheInvalidator<ProductCross, int> cacheInvalidator) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        await cacheInvalidator.Invalidate(context.Message.Id);
    }
}