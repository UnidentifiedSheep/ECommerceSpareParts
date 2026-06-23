using Analytics.Entities.Interfaces;
using Analytics.Enums;
using Domain.Extensions;
using Exceptions;
using Domain;

namespace Analytics.Entities;

public class SalesFact : Entity<SalesFact, Guid>, IDependency
{
    private SalesFact()
    {
    }

    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }

    public Guid BuyerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    public decimal TotalSum { get; private set; }

    public virtual ICollection<SaleContent> SaleContents { get; } = new List<SaleContent>();

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

        var existingContents = SaleContents.ToDictionary(x => x.Id);
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
                    incomingContent.Discount);
            }
            else
            {
                SaleContents.Add(incomingContent);
            }
        }

        foreach (var item in toRemove.Values)
            SaleContents.Remove(item);

        TotalSum = totalSum;
    }

    public static DependsOn DependsOn => DependsOn.Sale;
}
