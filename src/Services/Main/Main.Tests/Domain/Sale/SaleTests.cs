using Exceptions;
using FluentAssertions;
using Main.Entities.Sale;
using Main.Enums;
using DomainSale = Main.Entities.Sale.Sale;

namespace Tests.Domain.Sale;

public class SaleTests
{
    [Fact]
    public void Create_ValidData_CreatesDraftSale()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var saleDate = DateTime.UtcNow;

        var sale = DomainSale.Create(
            userId,
            organizationId,
            transactionId,
            1,
            "WH-1",
            saleDate);

        sale.UserId.Should().Be(userId);
        sale.OrganizationId.Should().Be(organizationId);
        sale.TransactionId.Should().Be(transactionId);
        sale.CurrencyId.Should().Be(1);
        sale.StorageName.Should().Be("WH-1");
        sale.SaleDatetime.Should().Be(saleDate);
        sale.State.Should().Be(SaleState.Draft);
        sale.Contents.Should().BeEmpty();
    }

    [Fact]
    public void AddContent_AddsContent()
    {
        var sale = Create();
        var content = CreateContent();

        sale.AddContent(content);

        sale.Contents.Should().ContainSingle().Which.Should().BeSameAs(content);
    }

    [Fact]
    public void AddContent_WithWrongSaleId_Throws()
    {
        var sale = Create();
        var content = CreateContent();
        typeof(SaleContent)
            .GetProperty(nameof(SaleContent.SaleId))!
            .SetValue(content, Guid.NewGuid());

        var act = () => sale.AddContent(content);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveContent_WithWrongSaleId_Throws()
    {
        var sale = Create();
        var content = CreateContent();
        typeof(SaleContent)
            .GetProperty(nameof(SaleContent.SaleId))!
            .SetValue(content, Guid.NewGuid());

        var act = () => sale.RemoveContent(content);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WithContent_ChangesState()
    {
        var sale = Create();
        sale.AddContent(CreateContent());

        sale.Complete();

        sale.State.Should().Be(SaleState.Completed);
    }

    [Fact]
    public void Complete_WithoutContent_Throws()
    {
        var sale = Create();

        var act = () => sale.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WhenDeleted_Throws()
    {
        var sale = Create();
        sale.Delete();

        var act = () => sale.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Delete_ChangesState()
    {
        var sale = Create();

        sale.Delete();

        sale.State.Should().Be(SaleState.Deleted);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_IsIdempotent()
    {
        var sale = Create();
        sale.Delete();

        var act = () => sale.Delete();

        act.Should().NotThrow();
        sale.State.Should().Be(SaleState.Deleted);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData(" test ", "test")]
    public void SetComment_HandlesCases(string? input, string? expected)
    {
        var sale = Create();

        sale.SetComment(input);

        sale.Comment.Should().Be(expected);
    }

    [Fact]
    public void SetComment_WhenTooLong_Throws()
    {
        var sale = Create();

        var act = () => sale.SetComment(new string('a', 257));

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetDateTime_UpdatesDate()
    {
        var sale = Create();
        var date = DateTime.UtcNow.AddDays(-1);

        sale.SetDateTime(date);

        sale.SaleDatetime.Should().Be(date);
    }

    [Fact]
    public void SetCurrency_UpdatesCurrency()
    {
        var sale = Create();

        sale.SetCurrency(2);

        sale.CurrencyId.Should().Be(2);
    }

    private static DomainSale Create()
    {
        return DomainSale.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "WH-1",
            DateTime.UtcNow);
    }

    private static SaleContent CreateContent()
    {
        return SaleContent.Create(
            1,
            100m,
            80m,
            [
                SaleContentDetail.Create(
                    1,
                    1,
                    10m,
                    2,
                    DateTime.UtcNow)
            ]);
    }
}
