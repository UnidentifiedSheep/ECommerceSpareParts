using Main.Core.Dtos.Amw.Users;
using Main.Core.Dtos.Currencies;

namespace Main.Core.Dtos.Amw.Purchase;

public class PurchaseDto
{
    public string Id { get; set; } = null!;
    public UserDto Supplier { get; set; } = null!;
    public CurrencyDto Currency { get; set; } = null!;
    public string? Comment { get; set; }
    public string Storage { get; set; } = null!;
    public DateTime PurchaseDatetime { get; set; }
    public string TransactionId { get; set; } = null!;
    public decimal TotalSum { get; set; }
}