using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Tests.Extensions;

public static class ProductStockExtensions
{
	public static Task<int> GetProductStockQuantityAsync(
		this DContext context, int productId, CancellationToken cancellationToken) =>
		context.ProductStocks.AsNoTracking().Where(stock => stock.ProductId == productId)
			.SumAsync(stock => stock.Quantity, cancellationToken);
}
