namespace Search.Persistence.Interfaces;

public interface IIndexInitializer<TDocument>
{
    Task LazyInitialize(CancellationToken cancellationToken = default);
}