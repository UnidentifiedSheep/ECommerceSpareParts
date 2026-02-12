using Contracts.Models.StorageContent;
using Contracts.StorageContent.GetStorageContentCosts;
using Main.Application.Handlers.StorageContents.GetStorageContentCosts;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Consumers;

public class GetStorageContentCostsConsumer(IMediator mediator) : IConsumer<GetStorageContentCostsRequest>
{
    public async Task Consume(ConsumeContext<GetStorageContentCostsRequest> context)
    {
        var result = await mediator.Send(new GetStorageContentCostsQuery(context.Message.ArticleIds,
                context.Message.OnlyPositiveQty));
        
        var adapted = result.StorageContentCosts.Adapt<List<StorageContentCost>>();
        await context.RespondAsync(new GetStorageContentCostsResponse { StorageContentCosts = adapted });
    }
}