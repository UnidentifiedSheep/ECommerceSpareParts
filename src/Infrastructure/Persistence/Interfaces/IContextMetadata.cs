using System.Linq.Expressions;

namespace Persistence.Interfaces;

public interface IContextMetadata
{
    string GetColumnName<TEntity>(LambdaExpression keySelector);
    string GetTableName<TEntity>();
    string? GetSchemaName<TEntity>();
}