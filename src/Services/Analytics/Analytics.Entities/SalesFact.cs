using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Exceptions;

namespace Analytics.Entities;

public class SalesFact : Entity<SalesFact, Guid>, ILinqEntity<SalesFact, Guid>
{
    private readonly List<SaleContent> _saleContents = [];

    private SalesFact() { }

    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }

    public int BaseCurrencyId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid BuyerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public decimal TotalSum { get; private set; }

    public decimal RevenueInBaseCurrency { get; private set; }

    public decimal CostInBaseCurrency { get; private set; }

    public decimal GrossProfitInBaseCurrency { get; private set; }

    public int ProductsCount { get; private set; }

    public IReadOnlyCollection<SaleContent> SaleContents => _saleContents;

    public static Expression<Func<SalesFact, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<SalesFact, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public override Guid GetId() { return Id; }

    public static SalesFact Create(
        Guid id,
        int currencyId,
        int baseCurrencyId,
        Guid organizationId,
        Guid buyerId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<SaleContent> contents)
    {
        var fact = new SalesFact
        {
            Id = id,
            CurrencyId = currencyId,
            BaseCurrencyId = baseCurrencyId,
            OrganizationId = organizationId,
            BuyerId = buyerId,
            CreatedAt = createdAt,
            ProcessedAt = processedAt
        };

        fact.ApplyContents(contents);

        return fact;
    }

    public static SalesFact CreateDeleted(Guid id, DateTime processedAt)
    {
        return new SalesFact
        {
            Id = id,
            CreatedAt = processedAt,
            ProcessedAt = processedAt,
            IsDeleted = true
        };
    }

    public void Update(
        int currencyId,
        int baseCurrencyId,
        Guid organizationId,
        Guid buyerId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<SaleContent> contents)
    {
        CurrencyId = currencyId;
        BaseCurrencyId = baseCurrencyId;
        OrganizationId = organizationId;
        BuyerId = buyerId;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
        IsDeleted = false;

        ApplyContents(contents);
    }

    public void MarkDeleted(DateTime processedAt)
    {
        ProcessedAt = processedAt;
        IsDeleted = true;
    }

    private void ApplyContents(IEnumerable<SaleContent> contents)
    {
        var incomingContents = contents
            .EnsureNotNull(() => new InvalidInputException("sale.fact.content.required"))
            .ToList()
            .EnsureNotEmpty(() => new InvalidInputException("sale.fact.content.required"));

        var existingContents = _saleContents.ToDictionary(x => x.Id);
        var toRemove = new Dictionary<int, SaleContent>(existingContents);
        var totalSum = 0m;
        var revenueInBaseCurrency = 0m;
        var costInBaseCurrency = 0m;
        var productsCount = 0;

        foreach (var incomingContent in incomingContents)
        {
            toRemove.Remove(incomingContent.Id);
            totalSum += incomingContent.TotalSum;
            revenueInBaseCurrency += incomingContent.PriceInBaseCurrency * incomingContent.Count;
            costInBaseCurrency += incomingContent.Details.Sum(
                detail => detail.BuyPriceInBaseCurrency * detail.Count);
            productsCount += incomingContent.Count;

            if (existingContents.TryGetValue(incomingContent.Id, out var existingContent))
                existingContent.Update(
                    incomingContent.ProductId,
                    incomingContent.Price,
                    incomingContent.PriceInBaseCurrency,
                    incomingContent.Count,
                    incomingContent.Discount,
                    incomingContent.Details);
            else
                _saleContents.Add(incomingContent);
        }

        foreach (var item in toRemove.Values) _saleContents.Remove(item);

        TotalSum = totalSum;
        RevenueInBaseCurrency = revenueInBaseCurrency;
        CostInBaseCurrency = costInBaseCurrency;
        GrossProfitInBaseCurrency = revenueInBaseCurrency - costInBaseCurrency;
        ProductsCount = productsCount;
    }
}
