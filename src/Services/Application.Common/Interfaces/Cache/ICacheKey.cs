namespace Application.Common.Interfaces;

public interface ICacheKey<TEntity>
{
    string KeyTemplate { get; }
    string FormatKey(params object?[] args);
}