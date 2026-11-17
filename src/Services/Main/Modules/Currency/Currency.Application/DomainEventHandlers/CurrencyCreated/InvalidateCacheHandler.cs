using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Modules.Currency.Application.Interfaces;

namespace Main.Modules.Currency.Application.DomainEventHandlers.CurrencyCreated;

public sealed class InvalidateCacheHandler(ICurrencyCacheRepository cacheRepository)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<Entities.Currency.Currency>>
{
	public override async Task Handle(
		Batch<EntityCreatedDomainEvent<Entities.Currency.Currency>> notification,
		CancellationToken cancellationToken)
		=> await cacheRepository.InvalidateAllCurrencies(cancellationToken);
}
