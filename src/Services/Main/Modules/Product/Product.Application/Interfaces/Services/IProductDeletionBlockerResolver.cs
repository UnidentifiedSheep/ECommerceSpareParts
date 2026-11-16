using Locan.Core.Interfaces;

namespace Main.Modules.Product.Application.Interfaces.Services;

public interface IProductDeletionBlockerResolver
{
	Task<ILocalizableMessage?> GetAsync(int productId, CancellationToken cancellationToken = default);
}
