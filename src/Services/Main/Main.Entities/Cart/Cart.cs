using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;

namespace Main.Entities.Cart;

public class Cart : AuditableEntity<Cart, (Guid, int)>
{
    [ValidateTuple("PK")]
    public Guid UserId { get; private set; }

    [ValidateTuple("PK")]
    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public Product.Product Product { get; private set; } = null!;
    
    private Cart() {}

    private Cart(Guid userId, int productId, int count)
    {
        UserId = userId;
        ProductId = productId;
        SetCount(count);
    }

    public static Cart Create(Guid userId, int productId, int count)
    {
        return new Cart(userId, productId, count);
    }

    public void SetCount(int count)
    {
        count.AgainstTooSmall(1, "position.count.must.be.greater.than.zero");
        Count = count;
    }
    
    public override (Guid, int) GetId() => (UserId, ProductId);
}