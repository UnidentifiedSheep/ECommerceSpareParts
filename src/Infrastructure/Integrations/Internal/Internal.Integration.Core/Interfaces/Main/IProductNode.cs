using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<InternalFullProduct?> GetFullProduct(int productId, CancellationToken cancellationToken = default);
}