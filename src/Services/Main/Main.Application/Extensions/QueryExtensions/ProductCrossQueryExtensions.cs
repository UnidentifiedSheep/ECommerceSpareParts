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
}