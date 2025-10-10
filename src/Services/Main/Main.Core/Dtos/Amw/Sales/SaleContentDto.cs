using Main.Core.Dtos.Amw.Articles;

namespace Main.Core.Dtos.Amw.Sales;

public class SaleContentDto
{
    public int Id { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public decimal Discount { get; set; }
    public ArticleDto Article { get; set; } = null!;
}