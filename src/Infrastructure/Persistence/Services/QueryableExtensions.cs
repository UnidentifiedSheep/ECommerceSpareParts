using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Services;

public class QueryableExtensions(IContextMetadata contextMetadata) : IQueryableExtensions
{
    public IQueryable<T> ConfigureTracking<T>(IQueryable<T> query, bool track) where T : class
    {
        return track ? query : query.AsNoTracking();
    }

    public IQueryable<T> ForUpdate<T>(
        IQueryable<T> query,
        bool forUpdate = true,
        bool skipLocked = false)
        where T : class
    {
        var schema = contextMetadata.GetSchemaName<T>();
        var tableName = contextMetadata.GetTableName<T>();

        var fullQualifiedTableName = string.IsNullOrWhiteSpace(schema)
            ? $"{tableName}"
            : $"{schema}.{tableName}";

        if (!forUpdate) return query;

        query = query.TagWith("ForUpdate")
            .TagWith($"ForUpdateOf:{fullQualifiedTableName}");

        return skipLocked
            ? query.TagWith("SkipLocked")
            : query;
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query, Criteria<T>? criteria) where T : class
    {
        if (criteria == null) return query;
        query = ConfigureTracking(query, criteria.Track);
        query = ForUpdate(
            query,
            criteria.ForUpdate,
            criteria.SkipLocked);

        foreach (var where in criteria.Wheres) query = query.Where(where);

        foreach (var i in criteria.Includes) query = query.Include(i);

        if (criteria.OrderBy is not null) query = criteria.OrderBy(query);

        if (!criteria.Size.HasValue) return query;

        if (criteria.Page.HasValue) query = query.Skip(criteria.Page.Value * criteria.Size.Value);

        query = query.Take(criteria.Size.Value);
        return query;
    }
}