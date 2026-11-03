using Contracts.Storage;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductStocksUpdatedConsumer(IProductRepository repository) : IConsumer<ProductStocksUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductStocksUpdatedEvent> context)
	{
		foreach (var products in context.Message.Items.Chunk(100))
			await repository.ApplyStockUpdates(
				products.Select(product => new ProductStockUpdate(
					product.ProductId, new Dictionary<string, int> { [context.Message.StorageCode] = product.Quantity },
					context.Message.OccurredAt, false)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductStocksUpdatedConsumerDefinition : ConsumerDefinition<ProductStocksUpdatedConsumer>
{
	public ProductStocksUpdatedConsumerDefinition()
	{
		ConcurrentMessageLimit = 1;
	}
}
