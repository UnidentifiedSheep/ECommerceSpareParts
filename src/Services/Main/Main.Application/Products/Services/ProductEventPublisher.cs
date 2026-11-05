using Application.Common.Interfaces.Events;
using Main.Application.Products.Interfaces;
using Main.Enums;

namespace Main.Application.Products.Services;

public sealed class ProductEventPublisher(IProductEventProvider provider, IIntegrationEventScope scope) : IProductEventPublisher
{
	public async Task PublishAsync(IEnumerable<int> productIds, ProductEventParts parts, CancellationToken cancellationToken)
	{
		foreach (var ids in productIds.Distinct().Order().Chunk(100))
			scope.AddRange(await provider.GetEventsAsync(ids, parts, cancellationToken));
	}
}
