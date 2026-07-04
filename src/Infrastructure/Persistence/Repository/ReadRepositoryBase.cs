using System.Data;
using Application.Common.Interfaces.Repositories;
using Dapper;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repository;

public abstract class ReadRepositoryBase<TContext, TEntity, TKey>(TContext ctx)
    : IReadRepository<TEntity, TKey>
    where TEntity : Entity<TEntity, TKey>
    where TKey : notnull
    where TContext : DbContext
{
    public IQueryable<TEntity> Query => ctx.Set<TEntity>().AsQueryable().AsNoTracking();

    public async Task<IEnumerable<T>> QuerySqlAsync<T>(
        string sql,
        object param)
    {
        var connection = ctx.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        return await connection.QueryAsync<T>(sql, param);
    }
}