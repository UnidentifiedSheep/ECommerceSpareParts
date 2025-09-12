using Application.Events;
using Core.Entities;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.EventHandlers;

public class ArticlesUpdatedCacheInvalidator(IRelatedDataRepository<Article> relatedDataRepository, ICache cache) : INotificationHandler<ArticlesUpdatedEvent>
{
    public async Task Handle(ArticlesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var relatedKeys = new HashSet<string>();
        foreach (var id in notification.ArticleIds)
            relatedKeys.UnionWith(await relatedDataRepository.GetRelatedDataKeys(id.ToString()));
        await cache.DeleteAsync(relatedKeys);
    }
}