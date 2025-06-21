namespace MonoliteUnicorn.Dtos.Amw.Sales;

public class SaleDto
{
    public string Id { get; set; } = null!;
    public string BuyerId { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime SaleDatetime { get; set; }
    public int CurrencyId { get; set; }
    public string TransactionId { get; set; } = null!;
    public decimal TotalSum { get; set; }
}