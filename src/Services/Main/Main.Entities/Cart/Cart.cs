using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Extensions;

namespace Main.Entities.Cart;

public class Cart : AuditableEntity<Cart, (Guid, int)>, ILinqEntity<Cart, (Guid, int)>
{
    private Cart()
    {
    }

    private Cart(Guid userId, int productId, int count)
    {
        UserId = userId;
        ProductId = productId;
        SetCount(count);
    }

    [ValidateTuple("PK")]
    public Guid UserId { get; }

    [ValidateTuple("PK")]
    public int ProductId { get; }

    public int Count { get; private set; }

    public Product.Product Product { get; private set; } = null!;

    public static Cart Create(Guid userId, int productId, int count)
    {
        return new Cart(userId, productId, count);
    }

    public void SetCount(int count)
    {
        count.AgainstLessOrEqual(0, "position.count.must.be.greater.than.zero");
        Count = count;
    }

    public override (Guid, int) GetId()
    {
        return (UserId, ProductId);
    }

    public static Expression<Func<Cart, bool>> GetEqualityExpression((Guid, int) key)
        => x => x.UserId == key.Item1 && x.ProductId == key.Item2;
}