using Locan.Core.Interfaces;

namespace Main.Modules.Product.Abstractions;

public interface IProductDeletionRule
{
	int Order { get; }

	Task<ILocalizableMessage?> CheckAsync(int productId, CancellationToken cancellationToken = default);
}
