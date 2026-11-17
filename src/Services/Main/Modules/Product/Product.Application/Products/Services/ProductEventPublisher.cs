using Application.Common.Interfaces.Events;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.Products.Services;

public sealed class ProductEventPublisher(IProductEventProvider provider, IIntegrationEventScope scope)
	: IProductEventPublisher
{
	public async Task PublishAsync(
		IEnumerable<int> productIds,
		ProductEventParts parts,
		CancellationToken cancellationToken)
	{
		foreach (var ids in productIds.Distinct().Order().Chunk(100))
			scope.AddRange(
				await provider.GetEventsAsync(
					ids,
					parts,
					cancellationToken));
	}
}
