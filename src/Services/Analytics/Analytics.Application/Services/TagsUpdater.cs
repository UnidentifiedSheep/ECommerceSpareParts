using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Models;
using Analytics.Entities.Interfaces;

namespace Analytics.Application.Services;

public class TagsUpdater(
    IMetricRepository metricRepository) : ITagsUpdater
{
    public async Task UpdateTags<TEntity>(
        TagUpdateContext<TEntity> context,
        CancellationToken cancellationToken = default)
        where TEntity : IDependency
    {
        await metricRepository.MarkDirtyAsync(
            TEntity.DependsOn,
            context.NewFactDatetime,
            context.PreviousFactDatetime,
            cancellationToken);
    }
}
