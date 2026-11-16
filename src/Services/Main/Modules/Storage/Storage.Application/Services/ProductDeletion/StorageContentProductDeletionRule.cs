using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Storage.Application.Services.ProductDeletion;

public sealed class StorageContentProductDeletionRule(IReadRepository<StorageContent, int> repository)
	: IProductDeletionRule
{
	public int Order => 20;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await repository.Query.AnyAsync(item => item.ProductId == productId, cancellationToken)
			? StorageContentProductDeletionBlockedMessage.Instance
			: null;
}
