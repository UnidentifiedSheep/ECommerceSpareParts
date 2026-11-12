using Main.Entities.Storage;
using Persistence.Context;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Tests.Extensions;

public static class ProductStockExtensions
{
	public static Task<int> GetProductStockQuantityAsync(
		this UnifiedDbContext context, int productId, CancellationToken cancellationToken) =>
		context.Set<ProductStock>().AsNoTracking().Where(stock => stock.ProductId == productId)
			.SumAsync(stock => stock.Quantity, cancellationToken);
}
