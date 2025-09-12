namespace Core.Entities;

public class SaleContent
{
    public int Id { get; set; }

    public string SaleId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal TotalSum { get; set; }

    public string? Comment { get; set; }

    public decimal Discount { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual BuySellPrice? BuySellPrice { get; set; }

    public virtual Sale Sale { get; set; } = null!;

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();
}