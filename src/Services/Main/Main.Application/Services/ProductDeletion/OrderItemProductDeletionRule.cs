using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Order;
using Main.Entities.Product;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.ProductDeletion;

public sealed class OrderItemProductDeletionRule(IReadRepository<OrderItem, int> repository) : IProductDeletionRule
{
	public int Order => 50;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await repository.Query.AnyAsync(item => item.ProductId == productId, cancellationToken)
			? ArticleDeletionOrderMessage.Instance
			: null;
}
