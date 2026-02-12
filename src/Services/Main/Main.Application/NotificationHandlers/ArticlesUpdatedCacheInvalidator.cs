using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Main.Abstractions.Models;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticlesUpdatedCacheInvalidator(IRelatedDataRepository<ArticleCross> relatedDataBase, ICache cache)
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