using Core.Interfaces.CacheRepositories;
using Core.StaticFunctions;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleWeightCacheInvalidator(ICache cache) : INotificationHandler<ArticleWeightUpdatedNotification>
{
    public async Task Handle(ArticleWeightUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var key = string.Format(CacheKeys.ArticleWeightCacheKey, notification.ArticleId);
        await cache.DeleteAsync(key);
    }
}