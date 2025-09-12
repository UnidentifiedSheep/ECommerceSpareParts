namespace Core.Dtos.Amw.StorageContents;

public class StorageContentDto
{
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal BuyPrice { get; set; }

    public int CurrencyId { get; set; }

    public decimal BuyPriceInUsd { get; set; }
    
    public DateTime PurchaseDatetime { get; set; }

}