using Main.Enums;

namespace Main.Application.Products.Interfaces;

public interface IProductEventProvider
{
	Task<IReadOnlyList<object>> GetEventsAsync(IEnumerable<int> productIds, ProductEventParts parts,
		CancellationToken cancellationToken = default);
}
