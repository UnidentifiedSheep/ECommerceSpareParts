namespace Search.Application.Interfaces.Product;

public interface IProductSearchDocumentProvider
{
    Task<Dictionary<int, Entities.Product?>> GetByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);
}