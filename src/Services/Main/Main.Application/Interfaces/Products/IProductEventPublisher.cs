using Main.Enums;

namespace Main.Application.Interfaces.Products;

public interface IProductEventPublisher
{
	Task PublishAsync(IEnumerable<int> productIds, ProductEventParts parts, CancellationToken cancellationToken);
}
