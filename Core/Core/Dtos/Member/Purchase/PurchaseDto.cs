namespace Core.Dtos.Member.Purchase;

public class PurchaseDto
{
    public string Id { get; set; } = null!;
    public DateTime PurchaseDatetime { get; set; }
    public int CurrencyId { get; set; }
    public decimal TotalSum { get; set; }
}