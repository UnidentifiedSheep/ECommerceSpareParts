using Application.Common.Interfaces;

namespace Application.Common.Abstractions;

public abstract class CacheKeyRegistryBase : ICacheKeyRegistry
{
    private readonly Dictionary<Type, Delegate> _registry = new();

    protected void RegisterKey<TEntity, TArgs>(Func<TArgs, string> factory)
    {
        _registry[typeof(TEntity)] = factory;
    }

    public string FormatKey<TEntity, TArgs>(TArgs args)
    {
        if (!_registry.TryGetValue(typeof(TEntity), out var del))
            throw new InvalidOperationException($"Key for {typeof(TEntity)} not found");

        return ((Func<TArgs, string>)del)(args);
    }
}