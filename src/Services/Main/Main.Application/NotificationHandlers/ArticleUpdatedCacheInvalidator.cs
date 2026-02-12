using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Main.Abstractions.Models;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleUpdatedCacheInvalidator(IRelatedDataRepository<ArticleCross> relatedDataBase, ICache cache)
    : INotificationHandler<ArticleUpdatedNotification>
{
    public async Task Handle(ArticleUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var relatedKeys = await relatedDataBase.GetRelatedDataKeys(notification.ArticleId.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}