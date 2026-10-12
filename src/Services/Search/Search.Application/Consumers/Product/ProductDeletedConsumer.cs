using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Product;

public class ProductDeletedConsumer(IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer)
	: IConsumer<ProductsDeletedEvent>
{
	public Task Consume(ConsumeContext<ProductsDeletedEvent> context)
	{
		return productIndexSynchronizer.Delete(
			context.Message.Ids.Distinct(),
			context.CancellationToken);
	}
}

public class ProductDeletedConsumerDefinition : ConsumerDefinition<ProductDeletedConsumer>
{
	public ProductDeletedConsumerDefinition()
	{
		ConcurrentMessageLimit = 1;
	}
}
