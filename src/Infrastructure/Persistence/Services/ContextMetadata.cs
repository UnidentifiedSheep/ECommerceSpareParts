using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Services;

public class ContextMetadata<TContext>(TContext context) : IContextMetadata
    where TContext : DbContext
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<(Type, string), string> ColumnNameCache = new();
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, string> TableNameCache = new();
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, string?> SchemaNameCache = new();

    public string GetColumnName<TEntity>(LambdaExpression keySelector)
    {
        var memberExpression = keySelector.Body as MemberExpression
                               ?? (keySelector.Body as UnaryExpression)?.Operand as MemberExpression
                               ?? throw new InvalidOperationException("KeySelector не указывает на свойство.");

        var entityType = typeof(TEntity);
        var propertyName = memberExpression.Member.Name;

        return ColumnNameCache.GetOrAdd((entityType, propertyName), _ =>
        {
            var efEntity = context.Model.FindEntityType(entityType)
                           ?? throw new InvalidOperationException($"Тип {entityType.Name} не найден в модели EF Core.");

            var property = efEntity.FindProperty(propertyName)
                           ?? throw new InvalidOperationException(
                               $"Свойство {propertyName} не найдено в сущности {entityType.Name}.");

            return property.GetColumnName() ??
                   throw new InvalidOperationException("Не удалось получить название колонки.");
        });
    }

    public string GetTableName<TEntity>()
    {
        var entityType = typeof(TEntity);
        return TableNameCache.GetOrAdd(entityType, _ =>
        {
            return context.Model.FindEntityType(entityType)?.GetTableName()
                   ?? throw new ArgumentNullException($"Сущность {entityType.Name} не относится к сущностям бд.");
        });
    }

    public string? GetSchemaName<TEntity>()
    {
        var entityType = typeof(TEntity);
        return SchemaNameCache.GetOrAdd(entityType, _ =>
        {
            var schema = context.Model.FindEntityType(entityType)?.GetSchema();
            return string.IsNullOrWhiteSpace(schema) ? "public" : schema;
        });
    }
}