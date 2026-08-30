using Main.Entities.Product;

namespace Main.Application.Extensions.QueryExtensions;

public static class ProductCrossQueryExtensions
{
    public static IQueryable<Product> GetCrosses(
        this IQueryable<ProductCross> query,
        int productId)
    {
        var left = query
            .Where(c => c.RightProductId == productId)
            .Select(c => c.LeftProduct);

        var right = query
            .Where(c => c.LeftProductId == productId)
            .Select(c => c.RightProduct);

        return left.Union(right);
    }

    public static IQueryable<ProductCross> GetCrosses(
        this IQueryable<ProductCross> query,
        IReadOnlySet<int> productIds)
    {
        return query.Where(x =>
            productIds.Contains(x.LeftProductId) ||
            productIds.Contains(x.RightProductId));
    }

    public static IReadOnlyDictionary<int, IReadOnlySet<int>> GetCrosses(
        this IEnumerable<ProductCross> crosses,
        IReadOnlySet<int> productIds)
    {
        var result = productIds.ToDictionary(
            productId => productId,
            _ => new HashSet<int>());

        foreach (var cross in crosses)
        {
            if (result.TryGetValue(cross.LeftProductId, out var leftCrosses))
                leftCrosses.Add(cross.RightProductId);

            if (result.TryGetValue(cross.RightProductId, out var rightCrosses))
                rightCrosses.Add(cross.LeftProductId);
        }

        return result.ToDictionary(
            item => item.Key, 
            IReadOnlySet<int> (item) => item.Value);
    }
}
