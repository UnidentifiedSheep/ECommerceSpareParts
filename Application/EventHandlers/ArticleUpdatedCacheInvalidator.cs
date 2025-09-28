using Application.Events;
using Core.Abstractions;
using Core.Entities;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.EventHandlers;

public class ArticleUpdatedCacheInvalidator(RelatedDataBase<Article> relatedDataBase, ICache cache)
    : INotificationHandler<ArticleUpdatedEvent>
{
    public async Task Handle(ArticleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var relatedKeys = await relatedDataBase.GetRelatedDataKeys(notification.ArticleId.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}