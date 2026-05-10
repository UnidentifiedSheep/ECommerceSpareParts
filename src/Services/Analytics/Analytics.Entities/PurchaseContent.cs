using Domain;
using Domain.Extensions;
using Exceptions;

namespace Analytics.Entities;

public class PurchaseContent : Entity<PurchaseContent, int>
{
    private PurchaseContent()
    {
    }

    public int Id { get; private set; }

    public Guid PurchaseId { get; private set; }

    public int ArticleId { get; private set; }

    public decimal Price { get; private set; }

    public int Count { get; private set; }

    public virtual PurchasesFact Purchase { get; private set; } = null!;

    public override int GetId()
    {
        return Id;
    }

    public static PurchaseContent Create(int id, Guid purchaseId, int articleId, decimal price, int count)
    {
        return new PurchaseContent
        {
            Id = id,
            PurchaseId = purchaseId,
            ArticleId = articleId,
            Price = ValidatePrice(price),
            Count = ValidateCount(count)
        };
    }

    public void Update(int articleId, decimal price, int count)
    {
        ArticleId = articleId;
        Price = ValidatePrice(price);
        Count = ValidateCount(count);
    }

    private static decimal ValidatePrice(decimal price)
    {
        return price.AgainstLessOrEqual(
            0m,
            () => new InvalidInputException("purchase.fact.content.price.required"));
    }

    private static int ValidateCount(int count)
    {
        return count.AgainstLessOrEqual(
            0,
            () => new InvalidInputException("purchase.fact.content.count.required"));
    }
}