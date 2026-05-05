using Main.Application.Handlers.Cart.DeleteFromCart;
using Main.Entities.Exceptions.Cart;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts;

namespace Tests.HandlersTests.Cart;

public class DeleteFromCartTests : IntegrationTest
{
    private Main.Entities.Cart.Cart _cartItem = null!;
    public DeleteFromCartTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<UsersTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _cartItem = await new CartItemBuilder(Faker)
            .WithProductId(GetContext<ProductTestContext>().Products[0].Id)
            .WithUserId(GetContext<UsersTestContext>().Users.First().Id)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task DeleteFromCart_ValidData_Succeeds()
    {
        var command = new DeleteFromCartCommand(_cartItem.UserId, _cartItem.ProductId);

        await Mediator.Send(command);

        var cartItem = await Context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == command.UserId && x.ProductId == command.ProductId);
        Assert.Null(cartItem);
    }

    [Fact]
    public async Task DeleteFromCart_ItemNotFound_ThrowsCartItemNotFoundException()
    {
        var command = new DeleteFromCartCommand(_cartItem.UserId, 999999);

        await Assert.ThrowsAsync<CartItemNotFoundException>(() => Mediator.Send(command));
    }
}