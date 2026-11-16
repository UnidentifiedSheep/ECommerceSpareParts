using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.Currency;

namespace Main.Application.DomainEventHandlers.CurrencyCreated;

public sealed class InvalidateCacheHandler(ICurrencyCacheRepository cacheRepository)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<Currency>>
{
	public override async Task Handle(
		Batch<EntityCreatedDomainEvent<Currency>> notification,
		CancellationToken cancellationToken)
		=> await cacheRepository.InvalidateAllCurrencies(cancellationToken);
}
