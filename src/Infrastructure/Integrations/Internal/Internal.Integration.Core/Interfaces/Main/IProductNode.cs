using Integrations.Common;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<Response<InternalFullProduct>> GetFullProduct(
        int productId,
        CancellationToken cancellationToken = default);
}