namespace Core.Dtos.Amw.Sales;

public class NewSaleContentDto
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal PriceWithDiscount { get; set; }
    public string? Comment { get; set; }
}