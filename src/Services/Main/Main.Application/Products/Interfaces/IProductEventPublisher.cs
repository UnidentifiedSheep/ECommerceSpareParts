using Main.Enums;

namespace Main.Application.Products.Interfaces;

public interface IProductEventPublisher
{
	Task PublishAsync(IEnumerable<int> productIds, ProductEventParts parts, CancellationToken cancellationToken);
}
