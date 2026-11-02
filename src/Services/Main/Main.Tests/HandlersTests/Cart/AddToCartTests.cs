using Main.Entities.Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Application.Static;
using Main.Entities.Product;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
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

	public override async ValueTask InitializeAsync()
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

		var act = () => Mediator.Send(
			new AddToCartCommand(
				user.Id,
				product.Id,
				count));

		await act.Should().NotThrowAsync();

		var cartItem = await Context
			.Carts
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProductId == product.Id, CancellationToken);
		Assert.NotNull(cartItem);
		Assert.Equal(count, cartItem.Count);
	}

	[Fact]
	public async Task AddToCart_SameItem_ThrowsSameItemInCartException()
	{
		var (user, product) = GetUserAndProduct();
		var command = new AddToCartCommand(
			user.Id,
			product.Id,
			5);
		await Mediator.Send(command, CancellationToken);

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));
		Assert.IsType<CartItemAlreadyExistsException>(exception.Failures[0].Error);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task AddToCart_InvalidCount_ThrowsValidationException(int count)
	{
		var (user, product) = GetUserAndProduct();
		var command = new AddToCartCommand(
			user.Id,
			product.Id,
			count);

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task AddToCart_UserNotFound_ThrowsUserNotFoundException()
	{
		var (_, product) = GetUserAndProduct();
		var command = new AddToCartCommand(
			Guid.NewGuid(),
			product.Id,
			1);

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));
		Assert.IsType<UserNotFoundException>(exception.Failures[0].Error);
	}

	[Fact]
	public async Task MissingProduct_ReturnsProductNotFoundWithoutAddingCartItem()
	{
		var user = _usersContext.User;

		var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
			Mediator.Send(new AddToCartCommand(user.Id, int.MaxValue, 1), CancellationToken));

		exception.Failures.Should().ContainSingle(failure => failure.Error is ProductNotFoundException);
		(await Context.Carts.AnyAsync(item => item.UserId == user.Id, CancellationToken)).Should().BeFalse();
	}

	private (User, Product) GetUserAndProduct() => (_usersContext.Users.First(), _productContext.Products[0]);
}
