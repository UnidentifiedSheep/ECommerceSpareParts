using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<InternalFullProduct?> GetFullProduct(int productId, CancellationToken cancellationToken = default);
}