using Abstractions.Interfaces.Cache;
using Main.Abstractions.Utils;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleSizeCacheInvalidator(ICache cache) : INotificationHandler<ArticleSizeUpdatedNotification>
{
    public async Task Handle(ArticleSizeUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var key = string.Format(CacheKeys.ArticleSizeCacheKey, notification.ArticleId);
        await cache.DeleteAsync(key);
    }
}