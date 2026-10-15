using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductDimensionsUpdatedConsumer(IProductRepository repository) : IConsumer<ProductDimensionsUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductDimensionsUpdatedEvent> context)
	{
		foreach (var products in context.Message.Products.Chunk(100))
			await repository.ApplyUpdates(
				products.Select(product => ProductDimensionsUpdate.From(product, context.Message.UpdatedAt)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductDimensionsUpdatedConsumerDefinition : ConsumerDefinition<ProductDimensionsUpdatedConsumer>
{
	public ProductDimensionsUpdatedConsumerDefinition() => ConcurrentMessageLimit = 1;
}
