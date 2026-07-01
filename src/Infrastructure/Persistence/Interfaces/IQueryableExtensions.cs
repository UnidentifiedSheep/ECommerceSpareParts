using Application.Common.Interfaces.Repositories;

namespace Persistence.Interfaces;

public interface IQueryableExtensions
{
    IQueryable<T> ConfigureTracking<T>(IQueryable<T> query, bool track) where T : class;

    IQueryable<T> ForUpdate<T>(
        IQueryable<T> query,
        bool forUpdate = true,
        bool skipLocked = false)
        where T : class;

    IQueryable<T> Apply<T>(IQueryable<T> query, Criteria<T>? criteria) where T : class;
}