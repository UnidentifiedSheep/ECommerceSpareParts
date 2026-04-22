using System.Data;
using Application.Common.Interfaces.Repositories;
using Dapper;
using Domain;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Main.Persistence.Repositories;

public class ReadRepository<TEntity, TKey>(DContext ctx)
    : IReadRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey>
{
    public IQueryable<TEntity> Query => ctx.Set<TEntity>().AsQueryable().AsNoTracking();
    
    public async Task<IEnumerable<T>> QuerySqlAsync<T>(
        string sql,
        object param)
    {
        var connection = ctx.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        return await connection.QueryAsync<T>(sql, param);
    }
}