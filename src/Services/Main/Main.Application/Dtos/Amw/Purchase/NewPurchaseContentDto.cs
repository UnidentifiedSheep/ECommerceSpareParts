namespace Main.Abstractions.Dtos.Amw.Purchase;

public class NewPurchaseContentDto
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public string? Comment { get; set; }
    public bool CalculateLogistics { get; set; }
}