using Exceptions;
using FluentAssertions;
using Main.Entities.Storage;
using Main.Enums;

namespace Tests.Domain.Storage;

public class ProductReservationTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var organizationId = Guid.NewGuid();

        var reservation = ProductReservation.Create(
            organizationId,
            10,
            3);

        reservation.OrganizationId.Should().Be(organizationId);
        reservation.ProductId.Should().Be(10);
        reservation.ReservedCount.Should().Be(3);
        reservation.CurrentCount.Should().Be(0);
        reservation.Status.Should().Be(ProductReservationStatus.Active);
        reservation.ProposedPrice.Should().BeNull();
        reservation.ProposedCurrencyId.Should().BeNull();
        reservation.Comment.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidReservedCount_Throws(int reservedCount)
    {
        var act = () => ProductReservation.Create(
            Guid.NewGuid(),
            10,
            reservedCount);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void ProposePrice_ValidData_SetsPriceAndCurrency()
    {
        var reservation = Create();

        reservation.ProposePrice(100.55m, 2);

        reservation.ProposedPrice.Should().Be(100.55m);
        reservation.ProposedCurrencyId.Should().Be(2);
    }

    [Fact]
    public void ProposePrice_NullPriceAndCurrency_ClearsProposal()
    {
        var reservation = Create();
        reservation.ProposePrice(100m, 2);

        reservation.ProposePrice(null, null);

        reservation.ProposedPrice.Should().BeNull();
        reservation.ProposedCurrencyId.Should().BeNull();
    }

    [Fact]
    public void ProposePrice_PriceWithoutCurrency_Throws()
    {
        var reservation = Create();

        var act = () => reservation.ProposePrice(100m, null);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void ProposePrice_CurrencyWithoutPrice_Throws()
    {
        var reservation = Create();

        var act = () => reservation.ProposePrice(null, 1);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10.123)]
    public void ProposePrice_InvalidPrice_Throws(decimal price)
    {
        var reservation = Create();

        var act = () => reservation.ProposePrice(price, 1);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(" test ", "test")]
    public void SetComment_HandlesNullBlankAndTrim(string? input, string? expected)
    {
        var reservation = Create();

        reservation.SetComment(input);

        reservation.Comment.Should().Be(expected);
    }

    [Fact]
    public void SetComment_TooLong_Throws()
    {
        var reservation = Create();
        var comment = new string('a', 501);

        var act = () => reservation.SetComment(comment);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void AddCount_PartialCount_SetsLockedStatus()
    {
        var reservation = Create();

        reservation.AddCount(2);

        reservation.CurrentCount.Should().Be(2);
        reservation.Status.Should().Be(ProductReservationStatus.Locked);
    }

    [Fact]
    public void AddCount_FullCount_SetsDoneStatus()
    {
        var reservation = Create();

        reservation.AddCount(3);

        reservation.CurrentCount.Should().Be(3);
        reservation.Status.Should().Be(ProductReservationStatus.Done);
    }

    [Fact]
    public void AddCount_DecreasedToZero_ReturnsToActiveStatus()
    {
        var reservation = Create();
        reservation.AddCount(2);

        reservation.AddCount(-2);

        reservation.CurrentCount.Should().Be(0);
        reservation.Status.Should().Be(ProductReservationStatus.Active);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    public void AddCount_InvalidResult_Throws(int amount)
    {
        var reservation = Create();

        var act = () => reservation.AddCount(amount);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProposePrice_LockedReservation_Succeeds()
    {
        var reservation = Create();
        reservation.AddCount(1);

        reservation.ProposePrice(100m, 1);

        reservation.ProposedPrice.Should().Be(100m);
        reservation.ProposedCurrencyId.Should().Be(1);
        reservation.Status.Should().Be(ProductReservationStatus.Locked);
    }

    [Fact]
    public void ProposePrice_DoneReservation_Throws()
    {
        var reservation = Create();
        reservation.AddCount(3);

        var act = () => reservation.ProposePrice(100m, 1);

        act.Should().Throw<InvalidInputException>();
        reservation.Status.Should().Be(ProductReservationStatus.Done);
    }

    [Fact]
    public void Cancel_SetsCanceledStatus()
    {
        var reservation = Create();

        reservation.Cancel();

        reservation.Status.Should().Be(ProductReservationStatus.Canceled);
    }

    [Fact]
    public void AddCount_CanceledReservation_Throws()
    {
        var reservation = Create();
        reservation.Cancel();

        var act = () => reservation.AddCount(1);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void ProposePrice_CanceledReservation_Throws()
    {
        var reservation = Create();
        reservation.Cancel();

        var act = () => reservation.ProposePrice(100m, 1);

        act.Should().Throw<InvalidInputException>();
    }

    private static ProductReservation Create(int reservedCount = 3)
    {
        return ProductReservation.Create(
            Guid.NewGuid(),
            10,
            reservedCount);
    }
}
