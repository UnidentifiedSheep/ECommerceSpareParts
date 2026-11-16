using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Order.Order;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Order.Application.Services;

public sealed class OrderItemProductDeletionRule(IReadRepository<OrderItem, int> repository) : IProductDeletionRule
{
	public int Order => 50;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await repository.Query.AnyAsync(item => item.ProductId == productId, cancellationToken)
			? OrderItemProductDeletionBlockedMessage.Instance
			: null;
}
