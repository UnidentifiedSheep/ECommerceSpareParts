using Domain;

namespace Analytics.Entities;

public class SaleContent : Entity<SaleContent, int>
{
    public int Id { get; set; }

    public Guid? SaleId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal Discount { get; set; }

    public virtual SalesFact? Sale { get; set; }
    public override int GetId() => Id;
}