using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Purchase;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.ProductDeletion;

public sealed class PurchaseContentProductDeletionRule(IReadRepository<PurchaseContent, int> repository) : IProductDeletionRule
{
	public int Order => 40;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await repository.Query.AnyAsync(item => item.ProductId == productId, cancellationToken)
			? PurchaseContentProductDeletionBlockedMessage.Instance
			: null;
}
