using Abstractions.Interfaces.Cache;
using Main.Abstractions.Constants;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleWeightCacheInvalidator(ICache cache) : INotificationHandler<ArticleWeightUpdatedNotification>
{
    public async Task Handle(ArticleWeightUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var key = string.Format(CacheKeys.ProductWeightCacheKey, notification.ArticleId);
        await cache.DeleteAsync(key);
    }
}