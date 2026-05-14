using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Interfaces;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static Task<Dictionary<TKey, TEntity>> EnsureExistsAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        IEnumerable<TKey> ids,
        Func<IReadOnlyList<TKey>, Exception> errorFactory,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        var criteria = Criteria<TEntity>.New()
            .Track()
            .Build();

        return repository.EnsureExistsCoreAsync(
            ids,
            errorFactory,
            criteria,
            ct);
    }

    public static Task<Dictionary<TKey, TEntity>> EnsureExistsForUpdateAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        IEnumerable<TKey> ids,
        Func<IReadOnlyList<TKey>, Exception> errorFactory,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        var criteria = Criteria<TEntity>.New()
            .Track()
            .ForUpdate()
            .Build();

        return repository.EnsureExistsCoreAsync(
            ids,
            errorFactory,
            criteria,
            ct);
    }

    private static async Task<Dictionary<TKey, TEntity>> EnsureExistsCoreAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        IEnumerable<TKey> ids,
        Func<IReadOnlyList<TKey>, Exception> errorFactory,
        Criteria<TEntity> criteria,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        var keySet = ids.ToHashSet();

        var result = await repository.FindByIdsAsync(
            keySet,
            criteria,
            ct);

        keySet.EnsureAllExists(
            result.Keys,
            errorFactory);

        return result;
    }

    public static async Task<TEntity> EnsureExistForUpdateAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        TKey key,
        Func<TKey, Exception> errorFactory,
        CriteriaBuilder<TEntity>? criteriaBuilder = null,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
        where TKey : notnull
        => await repository.EnsureExistCoreAsync(
            key, 
            errorFactory, 
            criteriaBuilder?.ForUpdate() ?? Criteria<TEntity>.New().ForUpdate(), 
            ct);
    
    public static async Task<TEntity> EnsureExistAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        TKey key,
        Func<TKey, Exception> errorFactory,
        CriteriaBuilder<TEntity>? criteriaBuilder = null,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
        where TKey : notnull
        => await repository.EnsureExistCoreAsync(
            key, 
            errorFactory, 
            criteriaBuilder ?? Criteria<TEntity>.New(), 
            ct);

    private static async Task<TEntity> EnsureExistCoreAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        TKey key,
        Func<TKey, Exception> errorFactory,
        CriteriaBuilder<TEntity> criteriaBuilder,
        CancellationToken ct = default) 
        where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
        where TKey : notnull
    {
        var criteria = criteriaBuilder
            .Where(TEntity.GetEqualityExpression(key))
            .Build();
        
        var result = await repository.FirstOrDefaultAsync(
            criteria,
            ct);
        
        return result ?? throw errorFactory(key);
    }
}