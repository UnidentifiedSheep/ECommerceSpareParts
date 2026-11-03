using Contracts.Storage;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductStocksSnapshotConsumer(IProductRepository repository) : IConsumer<ProductStocksSnapshotEvent>
{
	public async Task Consume(ConsumeContext<ProductStocksSnapshotEvent> context)
	{
		foreach (var products in context.Message.Products.Chunk(100))
			await repository.ApplyStockUpdates(
				products.Select(product => new ProductStockUpdate(
					product.ProductId, product.Stocks, context.Message.OccurredAt, true)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductStocksSnapshotConsumerDefinition : ConsumerDefinition<ProductStocksSnapshotConsumer>
{
	public ProductStocksSnapshotConsumerDefinition()
	{
		ConcurrentMessageLimit = 1;
	}
}
