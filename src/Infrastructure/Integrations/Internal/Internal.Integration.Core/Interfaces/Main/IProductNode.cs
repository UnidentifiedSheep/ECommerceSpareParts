using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<InternalResponse<InternalFullProduct>> GetFullProduct(int productId, CancellationToken cancellationToken = default);
}