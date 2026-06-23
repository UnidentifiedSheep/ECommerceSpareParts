using Domain;
using Domain.Extensions;
using Exceptions;

namespace Analytics.Entities;

public class SaleContent : Entity<SaleContent, int>
{
    private SaleContent()
    {
    }

    public int Id { get; private set; }

    public Guid SaleId { get; private set; }

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal Price { get; private set; }

    public decimal Discount { get; private set; }

    public decimal TotalSum => Count * Price - Discount;

    public virtual SalesFact Sale { get; private set; } = null!;

    public override int GetId()
    {
        return Id;
    }

    public static SaleContent Create(
        int id,
        Guid saleId,
        int productId,
        decimal price,
        int count,
        decimal discount)
    {
        return new SaleContent
        {
            Id = id,
            SaleId = saleId,
            ProductId = productId,
            Price = ValidatePrice(price),
            Count = ValidateCount(count),
            Discount = ValidateDiscount(discount)
        };
    }

    public void Update(int productId, decimal price, int count, decimal discount)
    {
        ProductId = productId;
        Price = ValidatePrice(price);
        Count = ValidateCount(count);
        Discount = ValidateDiscount(discount);
    }

    private static decimal ValidatePrice(decimal price)
    {
        return price.AgainstLessOrEqual(
            0m,
            () => new InvalidInputException("sale.fact.content.price.required"));
    }

    private static int ValidateCount(int count)
    {
        return count.AgainstLessOrEqual(
            0,
            () => new InvalidInputException("sale.fact.content.count.required"));
    }

    private static decimal ValidateDiscount(decimal discount)
    {
        return discount.AgainstNegative(
            () => new InvalidInputException("sale.fact.content.discount.must.not.be.negative"));
    }
}
