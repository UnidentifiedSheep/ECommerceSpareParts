using Bogus;
using Main.Entities.Cart;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class CartItemBuilder(Faker faker) : BuilderBase<Cart>(faker)
{
    public Guid? UserId { get; private set; }
    public int? ProductId { get; private set; }
    public int? Quantity { get; private set; }

    public CartItemBuilder WithUserId(Guid userId)
    {
        UserId = userId;
        return this;
    }

    public CartItemBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public CartItemBuilder WithQuantity(int quantity)
    {
        Quantity = quantity;
        return this;
    }
    
    public override Cart Build()
    {
        return Cart.Create(
            userId: UserId ?? Guid.NewGuid(),
            productId: ProductId ?? Faker.GlobalUniqueIndex,
            count: Quantity ?? Faker.Random.Int(1));
    }
}