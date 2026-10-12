using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Product;

public class ProductUpdatedConsumer(IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer)
	: IConsumer<ProductsUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductsUpdatedEvent> context)
	{
		foreach (var ids in context.Message.Ids.Distinct().Chunk(100))
			await productIndexSynchronizer.Reindex(ids, context.CancellationToken);
	}
}

public class ProductUpdatedConsumerDefinition : ConsumerDefinition<ProductUpdatedConsumer>
{
	public ProductUpdatedConsumerDefinition() => ConcurrentMessageLimit = 1;
}
