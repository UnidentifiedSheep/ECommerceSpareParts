namespace Application.Common.Interfaces;

public interface ICacheInvalidator<TEntity, TKey>
{
    Task Invalidate(TKey key);
    Task Invalidate(IEnumerable<TKey> keys);
}