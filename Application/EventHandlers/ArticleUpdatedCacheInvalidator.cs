using Application.Events;
using Core.Entities;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.EventHandlers;

public class ArticleUpdatedCacheInvalidator(IRelatedDataRepository<Article> relatedDataRepository, ICache cache)
    : INotificationHandler<ArticleUpdatedEvent>
{
    public async Task Handle(ArticleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var relatedKeys = await relatedDataRepository.GetRelatedDataKeys(notification.ArticleId.ToString());
        await cache.DeleteAsync(relatedKeys);
    }
}