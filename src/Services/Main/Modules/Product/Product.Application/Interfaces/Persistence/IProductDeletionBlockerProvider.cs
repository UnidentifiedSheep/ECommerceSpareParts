using Main.Enums.Products;

namespace Main.Application.Interfaces.Persistence;

public interface IProductDeletionBlockerProvider
{
	Task<ProductDeletionBlocker?> GetAsync(int productId, CancellationToken cancellationToken = default);
}
