namespace Analytics.Entities;

public class SaleContent
{
    public int Id { get; set; }

    public string? SaleId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal Discount { get; set; }

    public virtual SalesFact? Sale { get; set; }
}