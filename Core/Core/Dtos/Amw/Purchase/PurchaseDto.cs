namespace Core.Dtos.Amw.Purchase;

public class PurchaseDto
{
    public string Id { get; set; } = null!;
    public string SupplierId { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime PurchaseDatetime { get; set; }
    public int CurrencyId { get; set; }
    public string TransactionId { get; set; } = null!;
    public decimal TotalSum { get; set; }
}