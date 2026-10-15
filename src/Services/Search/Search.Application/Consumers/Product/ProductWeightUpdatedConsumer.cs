using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductWeightUpdatedConsumer(IProductRepository repository) : IConsumer<ProductWeightUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
	{
		foreach (var products in context.Message.Products.Chunk(100))
			await repository.ApplyUpdates(
				products.Select(product => ProductWeightUpdate.From(product, context.Message.UpdatedAt)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductWeightUpdatedConsumerDefinition : ConsumerDefinition<ProductWeightUpdatedConsumer>
{
	public ProductWeightUpdatedConsumerDefinition()
	{
		ConcurrentMessageLimit = 1;
	}
}
