using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Extensions;

namespace Main.Entities.Product;

public class ProductContent : Entity<ProductContent, (int, int)>, ILinqEntity<ProductContent, (int, int)>
{
    private ProductContent()
    {
    }

    private ProductContent(int parentProductId, int childProductId, int quantity)
    {
        parentProductId.AgainstEqual(childProductId, "article.content.self.reference.not.allowed");
        ParentProductId = parentProductId;
        ChildProductId = childProductId;
        SetQuantity(quantity);
    }

    public int ParentProductId { get; }
    public int ChildProductId { get; }
    public int Quantity { get; private set; }

    public Product ParentProduct { get; private set; } = null!;
    public Product ChildProduct { get; private set; } = null!;

    public static ProductContent Create(int parentProductId, int childProductId, int quantity)
    {
        return new ProductContent(parentProductId, childProductId, quantity);
    }

    public void SetQuantity(int quantity)
    {
        quantity.AgainstNegative("article.content.count.must.be.non.negative");
        Quantity = quantity;
    }

    public override (int, int) GetId()
    {
        return (ParentProductId, ChildProductId);
    }

    public static Expression<Func<ProductContent, bool>> GetEqualityExpression((int, int) key)
        => x => x.ParentProductId == key.Item1 && x.ChildProductId == key.Item2;
}