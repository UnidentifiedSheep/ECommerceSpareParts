using Locan.Core.Interfaces;

namespace Main.Application.Interfaces.Services;

public interface IProductDeletionBlockerResolver
{
	Task<ILocalizableMessage?> GetAsync(int productId, CancellationToken cancellationToken = default);
}
