using Main.Entities.Product.Enums;

namespace Main.Modules.Product.Application.Products.Interfaces;

public interface IProductEventPublisher
{
	Task PublishAsync(
		IEnumerable<int> productIds,
		ProductEventParts parts,
		CancellationToken cancellationToken);
}
