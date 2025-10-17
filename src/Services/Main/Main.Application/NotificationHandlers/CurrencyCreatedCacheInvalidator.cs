using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Application.Notifications;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class CurrencyCreatedCacheInvalidator(RelatedDataBase<Currency> relatedDataBase, ICache cache,
    ICurrencyRepository currencyRepository) 
    : INotificationHandler<CurrencyCreatedNotification>
{
    public async Task Handle(CurrencyCreatedNotification notification, CancellationToken cancellationToken)
    {
        var prevCurrency = await currencyRepository.GetCurrencyBeforeSpecifiedId(notification.Id, false, cancellationToken);
        if (prevCurrency == null) return;
        var relatedKeys = await relatedDataBase.GetRelatedDataKeys(prevCurrency.Id.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}