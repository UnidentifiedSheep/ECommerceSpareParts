using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Utils;
using Main.Application.Notifications;
using Main.Entities;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class CurrencyCreatedCacheInvalidator(IRelatedDataRepository<Currency> relatedDataBase, ICache cache,
    ICurrencyRepository currencyRepository) : INotificationHandler<CurrencyCreatedNotification>
{
    public async Task Handle(CurrencyCreatedNotification notification, CancellationToken cancellationToken)
    {
        var prevCurrency = await currencyRepository.GetCurrencyBeforeSpecifiedId(notification.Id, false, cancellationToken);
        if (prevCurrency == null) return;
        var relatedKeys = (await relatedDataBase.GetRelatedDataKeys(prevCurrency.Id.ToString()))
            .ToList();
        relatedKeys.Add(CacheKeys.CurrencyRatesCacheKey);
        await cache.DeleteAsync(relatedKeys);
    }
}