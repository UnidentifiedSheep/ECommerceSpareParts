using Core.Interfaces.CacheRepositories;
using Core.StaticFunctions;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class CurrencyRatesCacheInvalidator(ICache cache) : INotificationHandler<CurrencyRatesUpdatedNotification>
{
    public async Task Handle(CurrencyRatesUpdatedNotification notification, CancellationToken cancellationToken)
    {
        await cache.DeleteAsync(CacheKeys.CurrencyRatesCacheKey);
    }
}