using Domain;

namespace Analytics.Entities;

public class PurchaseContent : Entity<PurchaseContent, int>
{
    public int Id { get; set; }

    public string PurchaseId { get; set; } = null!;

    public int ArticleId { get; set; }

    public decimal Price { get; set; }

    public int Count { get; set; }

    public virtual PurchasesFact Purchase { get; set; } = null!;
    public override int GetId() => Id;
}