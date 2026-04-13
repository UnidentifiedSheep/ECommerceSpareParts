using Domain;
using Domain.Extensions;

namespace Main.Entities.Product;

public class ProductContent : Entity<ProductContent, (int, int)>
{
    public int ParentProductId { get; private set; }
    public int ChildProductId { get; private set; }
    public int Quantity { get; private set; }

    public Product ParentProduct { get; private set; } = null!;
    public Product ChildProduct { get; private set; } = null!;
    
    private ProductContent() {}

    private ProductContent(int parentProductId, int childProductId, int quantity)
    {
        parentProductId.AgainstEqual(childProductId, "article.content.self.reference.not.allowed");
        ParentProductId = parentProductId;
        ChildProductId = childProductId;
        SetQuantity(quantity);
    }

    public static ProductContent Create(int parentProductId, int childProductId, int quantity)
    {
        return new ProductContent(parentProductId, childProductId, quantity);
    }

    public void SetQuantity(int quantity)
    {
        quantity.AgainstNegative("article.content.count.must.be.non.negative");
        Quantity = quantity;
    }
    
    public override (int, int) GetId() => (ParentProductId, ChildProductId);
}