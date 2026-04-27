using Abstractions.Interfaces.Cache;
using Application.Common.Interfaces;
using Contracts.Articles;
using Main.Abstractions.Constants;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using MassTransit;

namespace Main.Application.Consumers;

public class ProductWeightUpdatedConsumer(
    ICacheInvalidator<GetProductWeightResult, int> cacheInvalidator) : IConsumer<ProductWeightUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
    {
        await cacheInvalidator.Invalidate(context.Message.ProductId);
    }
}