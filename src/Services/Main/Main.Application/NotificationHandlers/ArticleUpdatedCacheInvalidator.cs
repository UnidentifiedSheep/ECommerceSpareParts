using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Application.Notifications;
using Main.Core.Entities;
using Main.Core.Models;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleUpdatedCacheInvalidator(RelatedDataBase<ArticleCross> relatedDataBase, ICache cache)
    : INotificationHandler<ArticleUpdatedNotification>
{
    public async Task Handle(ArticleUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var relatedKeys = await relatedDataBase.GetRelatedDataKeys(notification.ArticleId.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}