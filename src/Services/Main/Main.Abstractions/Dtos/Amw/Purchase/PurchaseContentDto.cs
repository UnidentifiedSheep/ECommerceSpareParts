using Main.Abstractions.Dtos.Amw.Articles;

namespace Main.Abstractions.Dtos.Amw.Purchase;

public class PurchaseContentDto
{
    public int Id { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public ArticleDto Article { get; set; } = null!;
    public PurchaseContentLogisticDto? ContentLogisticDto { get; set; }
}