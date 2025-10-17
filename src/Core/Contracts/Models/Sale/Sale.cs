namespace Contracts.Models.Sale;

public class Sale
{
    public string Id { get; set; } = null!;
    public Guid CreatedUserId { get; set; }
    public Guid BuyerId { get; set; }
    public string? Comment { get; set; }
    public DateTime SaleDatetime { get; set; }
    public DateTime CreationDatetime { get; set; }
    public int CurrencyId { get; set; }
    public string TransactionId { get; set; } = null!;
    public string MainStorageName { get; set; } = null!;
    public List<SaleContent> SaleContents { get; set; } = null!;
}