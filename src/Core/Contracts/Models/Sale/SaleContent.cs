namespace Contracts.Models.Sale;

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
    public List<SaleContentDetail> Details { get; set; } = null!;
}