using FluentAssertions;
using Main.Abstractions.Constants;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Entities.Product;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Cart;

public class AddToCartTests : IntegrationTest
{
    private ProductTestContext _productContext = null!;
    private UsersTestContext _usersContext = null!;

    public AddToCartTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
        RegisterBasicContext<ProductTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _usersContext = GetContext<UsersTestContext>();
        _productContext = GetContext<ProductTestContext>();
    }

    [Fact]
    public async Task AddToCart_ValidData_Succeeds()
    {
        var (user, product) = GetUserAndProduct();
        var count = Faker.Random.Int(1, 100);

        var act = () => Mediator.Send(new AddToCartCommand(user.Id, product.Id, count));

        await act.Should().NotThrowAsync();

        var cartItem = await Context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProductId == product.Id);
        Assert.NotNull(cartItem);
        Assert.Equal(count, cartItem.Count);
    }

    [Fact]
    public async Task AddToCart_SameItem_ThrowsSameItemInCartException()
    {
        var (user, product) = GetUserAndProduct();
        var command = new AddToCartCommand(user.Id, product.Id, 5);
        await Mediator.Send(command);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
        Assert.Equal(ApplicationErrors.CartItemAlreadyExist, exception.Failures[0].ErrorName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddToCart_InvalidCount_ThrowsValidationException(int count)
    {
        var (user, product) = GetUserAndProduct();
        var command = new AddToCartCommand(user.Id, product.Id, count);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task AddToCart_UserNotFound_ThrowsUserNotFoundException()
    {
        var (_, product) = GetUserAndProduct();
        var command = new AddToCartCommand(Guid.NewGuid(), product.Id, 1);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }

    private (User, Product) GetUserAndProduct()
    {
        return (_usersContext.Users.First(), _productContext.Products[0]);
    }
}