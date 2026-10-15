using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces.Product;
using Search.Application.Models.ProductUpdates;

namespace Search.Application.Consumers.Product;

public class ProductCharacteristicsUpdatedConsumer(IProductRepository repository) : IConsumer<ProductCharacteristicsUpdatedEvent>
{
	public async Task Consume(ConsumeContext<ProductCharacteristicsUpdatedEvent> context)
	{
		foreach (var products in context.Message.Products.Chunk(100))
			await repository.ApplyUpdates(
				products.Select(product => ProductCharacteristicsUpdate.From(product, context.Message.UpdatedAt)).ToArray(),
				context.CancellationToken);
	}
}

public class ProductCharacteristicsUpdatedConsumerDefinition : ConsumerDefinition<ProductCharacteristicsUpdatedConsumer>
{
	public ProductCharacteristicsUpdatedConsumerDefinition() => ConcurrentMessageLimit = 1;
}
