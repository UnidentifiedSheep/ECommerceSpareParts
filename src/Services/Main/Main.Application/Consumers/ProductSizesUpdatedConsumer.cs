using Application.Common.Interfaces;
using Contracts.Articles;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using MassTransit;

namespace Main.Application.Consumers;

public class ProductSizesUpdatedConsumer(
    ICacheInvalidator<GetProductSizesResult, int> cacheInvalidator) : IConsumer<ProductSizesUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductSizesUpdatedEvent> context)
    {
        await cacheInvalidator.Invalidate(context.Message.ProductId);
    }
}