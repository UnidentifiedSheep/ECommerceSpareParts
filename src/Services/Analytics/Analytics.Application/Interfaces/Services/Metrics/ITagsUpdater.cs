using Analytics.Application.Models;
using Analytics.Entities.Interfaces;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface ITagsUpdater
{
    Task UpdateTags<TEntity>(
        TagUpdateContext<TEntity> context,
        CancellationToken cancellationToken = default)
        where TEntity : IDependency;
}