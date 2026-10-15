using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductUpdatedConsumer(IProductRepository repository) : IConsumer<ProductsUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductsUpdatedEvent> context)
	{
		foreach (var products in context.Message.Products.Chunk(100))
			await repository.ApplyUpdates(
				products.Select(product => ProductCoreUpdate.From(product, context.Message.UpdatedAt)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductUpdatedConsumerDefinition : ConsumerDefinition<ProductUpdatedConsumer>
{
	public ProductUpdatedConsumerDefinition()
	{
		ConcurrentMessageLimit = 1;
	}
}
