using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities.Cart;

public class Cart : AuditableEntity<Cart, (Guid, int)>
{
    [ValidateTuple("PK")]
    public Guid UserId { get; set; }

    [ValidateTuple("PK")]
    public int ProductId { get; set; }

    public int Count { get; set; }

    public virtual Product.Product Product { get; set; } = null!;
    
    public override (Guid, int) GetId() => (UserId, ProductId);
}