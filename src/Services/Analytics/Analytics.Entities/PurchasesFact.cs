using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Exceptions;

namespace Analytics.Entities;

public class PurchasesFact : Entity<PurchasesFact, Guid>
{
    private PurchasesFact()
    {
    }

    [Validate]
    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }

    public Guid SupplierId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    public decimal TotalSum { get; private set; }

    public virtual ICollection<PurchaseContent> PurchaseContents { get; } = new List<PurchaseContent>();

    public override Guid GetId()
    {
        return Id;
    }

    public static PurchasesFact Create(
        Guid id,
        int currencyId,
        Guid supplierId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<PurchaseContent> contents)
    {
        var fact = new PurchasesFact
        {
            Id = id,
            CurrencyId = currencyId,
            SupplierId = supplierId,
            CreatedAt = createdAt,
            ProcessedAt = processedAt
        };

        fact.ApplyContents(contents);

        return fact;
    }

    public void Update(
        int currencyId,
        Guid supplierId,
        DateTime createdAt,
        DateTime processedAt,
        IEnumerable<PurchaseContent> contents)
    {
        CurrencyId = currencyId;
        SupplierId = supplierId;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;

        ApplyContents(contents);
    }

    private void ApplyContents(IEnumerable<PurchaseContent> contents)
    {
        var incomingContents = contents
            .AgainstNull(() => new InvalidInputException("purchase.fact.content.required"))
            .ToList();

        incomingContents
            .AgainstEmpty(() => new InvalidInputException("purchase.fact.content.required"));

        var existingContents = PurchaseContents.ToDictionary(x => x.Id);
        var toRemove = new Dictionary<int, PurchaseContent>(existingContents);
        var totalSum = 0m;

        foreach (var incomingContent in incomingContents)
        {
            toRemove.Remove(incomingContent.Id);
            totalSum += incomingContent.Count * incomingContent.Price;

            if (existingContents.TryGetValue(incomingContent.Id, out var existingContent))
                existingContent.Update(incomingContent.ArticleId, incomingContent.Price, incomingContent.Count);
            else
                PurchaseContents.Add(incomingContent);
        }

        foreach (var item in toRemove.Values)
            PurchaseContents.Remove(item);

        TotalSum = totalSum;
    }
}