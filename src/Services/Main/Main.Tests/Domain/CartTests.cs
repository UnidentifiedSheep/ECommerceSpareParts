using Exceptions;
using FluentAssertions;
using Main.Entities.Cart;

namespace Tests.Domain;

public class CartTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var userId = Guid.NewGuid();

        var cart = Cart.Create(userId, 1, 2);

        cart.UserId.Should().Be(userId);
        cart.ProductId.Should().Be(1);
        cart.Count.Should().Be(2);
    }

    [Fact]
    public void Create_InvalidCount_Throws()
    {
        var userId = Guid.NewGuid();

        var act = () => Cart.Create(userId, 1, 0);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetCount_Valid_UpdatesValue()
    {
        var cart = Cart.Create(Guid.NewGuid(), 1, 2);

        cart.SetCount(5);

        cart.Count.Should().Be(5);
    }

    [Fact]
    public void SetCount_LessThanOne_Throws()
    {
        var cart = Cart.Create(Guid.NewGuid(), 1, 2);

        var act1 = () => cart.SetCount(0);
        var act2 = () => cart.SetCount(-10);

        act1.Should().Throw<InvalidInputException>();
        act2.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void GetId_ReturnsCompositeKey()
    {
        var userId = Guid.NewGuid();

        var cart = Cart.Create(userId, 99, 1);

        var id = cart.GetId();

        id.Item1.Should().Be(userId);
        id.Item2.Should().Be(99);
    }
}