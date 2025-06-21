using MonoliteUnicorn.Enums;

namespace MonoliteUnicorn.Dtos.Amw.Balances;

public class TransactionDto
{
    public string Id { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public int CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionStatus Status { get; set; }
}