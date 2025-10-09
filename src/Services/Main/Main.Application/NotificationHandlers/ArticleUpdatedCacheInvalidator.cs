using Core.Abstractions;
using Core.Entities;
using Core.Interfaces.CacheRepositories;
using Main.Application.Events;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticleUpdatedCacheInvalidator(RelatedDataBase<ArticleCross> relatedDataBase, ICache cache)
    : INotificationHandler<ArticleUpdatedEvent>
{
    public async Task Handle(ArticleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var relatedKeys = await relatedDataBase.GetRelatedDataKeys(notification.ArticleId.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}