using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Analytics;

internal static class SaleAnalyticsQueryExtensions
{
	public static async Task EnsureAnalyticsAsync(this IQueryable<Sale> query, CancellationToken cancellationToken)
	{
		if (await query.AnyAsync(sale => sale.Contents.Any(content => content.Analytics == null), cancellationToken))
			throw new InvalidOperationException("Sales contain missing analytics. Restore historical analytics before querying reports.");
	}
}
