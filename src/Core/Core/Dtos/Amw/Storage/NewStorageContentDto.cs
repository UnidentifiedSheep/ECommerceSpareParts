namespace Core.Dtos.Amw.Storage;

public class NewStorageContentDto
{
    public int ArticleId { get; set; }
    public int CurrencyId { get; set; }
    public decimal BuyPrice { get; set; }
    public int Count { get; set; }
    public DateTime PurchaseDate { get; set; }
}