using System.Linq.Expressions;
using Analytics.Entities.Interfaces;
using Analytics.Enums;
using Domain.Extensions;
using Exceptions;
using Domain;
using Domain.Interfaces;

namespace Analytics.Entities;

public class SalesFact : Entity<SalesFact, Guid>, IDependency, ILinqEntity<SalesFact, Guid>
{
    private readonly List<SaleContent> _saleContents = [];

    private SalesFact()
    {
    }

    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }

    public Guid BuyerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    public decimal TotalSum { get; private set; }

    public IReadOnlyCollection<SaleContent> SaleContents => _saleContents;

    public override Guid GetId()
    {
        return Id;
    }

    public static SalesFact Create(
        Guid id,
        int currencyId,
        Guid buyerId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<SaleContent> contents)
    {
        var fact = new SalesFact
        {
            Id = id,
            CurrencyId = currencyId,
            BuyerId = buyerId,
            CreatedAt = createdAt,
            ProcessedAt = processedAt
        };

        fact.ApplyContents(contents);

        return fact;
    }

    public void Update(
        int currencyId,
        Guid buyerId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<SaleContent> contents)
    {
        CurrencyId = currencyId;
        BuyerId = buyerId;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;

        ApplyContents(contents);
    }

    private void ApplyContents(IEnumerable<SaleContent> contents)
    {
        var incomingContents = contents
            .AgainstNull(() => new InvalidInputException("sale.fact.content.required"))
            .ToList()
            .AgainstEmpty(() => new InvalidInputException("sale.fact.content.required"));

        var existingContents = _saleContents.ToDictionary(x => x.Id);
        var toRemove = new Dictionary<int, SaleContent>(existingContents);
        var totalSum = 0m;

        foreach (var incomingContent in incomingContents)
        {
            toRemove.Remove(incomingContent.Id);
            totalSum += incomingContent.TotalSum;

            if (existingContents.TryGetValue(incomingContent.Id, out var existingContent))
            {
                existingContent.Update(
                    incomingContent.ProductId,
                    incomingContent.Price,
                    incomingContent.Count,
                    incomingContent.Discount,
                    incomingContent.Details);
            }
            else
            {
                _saleContents.Add(incomingContent);
            }
        }

        foreach (var item in toRemove.Values)
            _saleContents.Remove(item);

        TotalSum = totalSum;
    }

    public static DependsOn DependsOn => DependsOn.Sale;
    public static Expression<Func<SalesFact, Guid>> GetKeySelector() => x => x.Id;

    public static Expression<Func<SalesFact, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;
}
