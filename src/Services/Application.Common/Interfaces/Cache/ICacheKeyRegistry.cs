namespace Application.Common.Interfaces;

public interface ICacheKeyRegistry
{
    string FormatKey<TEntity, TArgs>(TArgs args);
}