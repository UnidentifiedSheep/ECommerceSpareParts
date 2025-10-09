namespace Core.Dtos.Amw.Purchase;

public class EditPurchaseDto
{
    public int? Id { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public string? Comment { get; set; }
}