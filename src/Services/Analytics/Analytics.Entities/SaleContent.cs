using Domain;
using Domain.Extensions;
using Exceptions;

namespace Analytics.Entities;

public class SaleContent : Entity<SaleContent, int>
{
    private readonly List<SaleContentDetail> _details = [];

    private SaleContent() { }

    public int Id { get; private set; }

    public Guid SaleId { get; private set; }

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal Price { get; private set; }

    public decimal PriceInBaseCurrency { get; private set; }

    public decimal Discount { get; private set; }

    public decimal TotalSum => Count * Price;

    public virtual SalesFact Sale { get; private set; } = null!;

    public IReadOnlyCollection<SaleContentDetail> Details => _details;

    public override int GetId() { return Id; }

    public static SaleContent Create(
        int id,
        Guid saleId,
        int productId,
        decimal price,
        decimal priceInBaseCurrency,
        int count,
        decimal discount,
        IEnumerable<SaleContentDetail>? details = null)
    {
        var content = new SaleContent
        {
            Id = id,
            SaleId = saleId,
            ProductId = productId,
            Price = ValidatePrice(price),
            PriceInBaseCurrency = ValidatePrice(priceInBaseCurrency),
            Count = ValidateCount(count),
            Discount = ValidateDiscount(discount)
        };

        content.ApplyDetails(details ?? []);
        return content;
    }

    public void Update(
        int productId,
        decimal price,
        decimal priceInBaseCurrency,
        int count,
        decimal discount,
        IEnumerable<SaleContentDetail>? details = null)
    {
        ProductId = productId;
        Price = ValidatePrice(price);
        PriceInBaseCurrency = ValidatePrice(priceInBaseCurrency);
        Count = ValidateCount(count);
        Discount = ValidateDiscount(discount);
        ApplyDetails(details ?? []);
    }

    private void ApplyDetails(IEnumerable<SaleContentDetail> details)
    {
        var incomingDetails = details
            .AgainstNull(() => new InvalidInputException("sale.fact.content.detail.required"))
            .ToList();

        var existingDetails = _details.ToDictionary(x => x.Id);
        var toRemove = new Dictionary<int, SaleContentDetail>(existingDetails);

        foreach (var incomingDetail in incomingDetails)
        {
            toRemove.Remove(incomingDetail.Id);

            if (existingDetails.TryGetValue(incomingDetail.Id, out var existingDetail))
                existingDetail.Update(
                    incomingDetail.CurrencyId,
                    incomingDetail.BuyPrice,
                    incomingDetail.BuyPriceInBaseCurrency,
                    incomingDetail.Count,
                    incomingDetail.PurchaseDate);
            else
                _details.Add(incomingDetail);
        }

        foreach (var item in toRemove.Values) _details.Remove(item);
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
        return discount.AgainstNegative(() =>
            new InvalidInputException("sale.fact.content.discount.must.not.be.negative"));
    }
}