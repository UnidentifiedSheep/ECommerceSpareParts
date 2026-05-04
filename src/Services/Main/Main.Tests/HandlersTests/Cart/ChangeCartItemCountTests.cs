using Main.Application.Handlers.Cart.ChangeCartItemCount;
using Main.Entities.Exceptions.Cart;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Cart;

public class ChangeCartItemCountTests : Test
{
    private Main.Entities.Cart.Cart _cartItem = null!;
    public ChangeCartItemCountTests(CombinedContainerFixture fixture) : base(fixture)
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
    public async Task ChangeCartItemCount_ValidData_Succeeds()
    {
        var newCount = Faker.Random.Int(1, 100);
        var command = new ChangeCartItemCountCommand(_cartItem.UserId, _cartItem.ProductId, newCount);

        await Mediator.Send(command);

        var cartItem = await Context.Carts
            .FirstOrDefaultAsync(x => x.UserId == command.UserId && x.ProductId == command.ProductId);
        Assert.NotNull(cartItem);
        Assert.Equal(newCount, cartItem.Count);
    }

    [Fact]
    public async Task ChangeCartItemCount_ItemNotFound_ThrowsCartItemNotFoundException()
    {
        var command = new ChangeCartItemCountCommand(_cartItem.UserId, 999999, 10);

        await Assert.ThrowsAsync<CartItemNotFoundException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ChangeCartItemCount_InvalidCount_ThrowsValidationException(int newCount)
    {
        var command = new ChangeCartItemCountCommand(_cartItem.UserId, _cartItem.ProductId, newCount);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }
}