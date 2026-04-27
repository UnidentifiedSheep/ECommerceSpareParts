using Application.Common.Interfaces;

namespace Application.Common.Abstractions;

public abstract class CacheKeyBase<TEntity> : ICacheKey<TEntity>
{
    public abstract string KeyTemplate { get; }
    public virtual string FormatKey(params object?[] args)
    {
        return string.Format(KeyTemplate, args);
    }
}