using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Storage;
using Main.Entities.Product;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.ProductDeletion;

public sealed class StorageContentProductDeletionRule(IReadRepository<StorageContent, int> repository) : IProductDeletionRule
{
	public int Order => 20;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await repository.Query.AnyAsync(item => item.ProductId == productId, cancellationToken)
			? ArticleDeletionStorageContentMessage.Instance
			: null;
}
