namespace Core.Dtos.Amw.Sales;

public class SaleContentDetailDto
{
    public int Id { get; set; }
    
    public int SaleContentId { get; set; }

    public int? StorageContentId { get; set; }

    public string Storage { get; set; } = null!;

    public int CurrencyId { get; set; }

    public decimal BuyPrice { get; set; }

    public int Count { get; set; }

    public DateTime PurchaseDatetime { get; set; }
}