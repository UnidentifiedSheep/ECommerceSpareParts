using Domain;
using Exceptions;

namespace Main.Entities.Product;

public class ProductCross : Entity<ProductCross, (int, int)>
{
    public int LeftProductId { get; private set; }
    public int RightProductId { get; private set; }

    public Product LeftProduct { get; set; } = null!;
    public Product RightProduct { get; set; } = null!;
    
    private ProductCross() {}

    private ProductCross(int left, int right)
    {
        if (left == right)
            throw new InvalidInputException("article.linkage.article.cannot.equal.cross.article");
        var min = Math.Min(left, right);
        var max = Math.Max(left, right);
        
        LeftProductId = min;
        RightProductId = max;
    }

    public static ProductCross Create(int id, int crossId)
    {
        return new ProductCross(id, crossId);
    }

    public override (int, int) GetId() => (LeftProductId, RightProductId);
}