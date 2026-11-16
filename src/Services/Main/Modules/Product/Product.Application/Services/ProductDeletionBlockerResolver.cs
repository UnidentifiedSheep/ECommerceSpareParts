using Locan.Core.Interfaces;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Application.Interfaces.Services;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductDeletionBlockerResolver(IEnumerable<IProductDeletionRule> rules)
	: IProductDeletionBlockerResolver
{
	public async Task<ILocalizableMessage?> GetAsync(
		int productId,
		CancellationToken cancellationToken = default)
	{
		foreach (var rule in rules.OrderBy(rule => rule.Order))
			if (await rule.CheckAsync(productId, cancellationToken) is { } message)
				return message;

		return null;
	}
}
