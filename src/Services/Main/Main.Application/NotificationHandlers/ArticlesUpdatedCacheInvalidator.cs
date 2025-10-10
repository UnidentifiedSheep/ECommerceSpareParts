using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Application.Notifications;
using Main.Core.Entities;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticlesUpdatedCacheInvalidator(RelatedDataBase<ArticleCross> relatedDataBase, ICache cache)
    : INotificationHandler<ArticlesUpdatedNotification>
{
    public async Task Handle(ArticlesUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var relatedKeys = new HashSet<string>();
        foreach (var id in notification.ArticleIds)
            relatedKeys.UnionWith(await relatedDataBase.GetRelatedDataKeys(id.ToString()));
        await cache.DeleteAsync(relatedKeys);
    }
}