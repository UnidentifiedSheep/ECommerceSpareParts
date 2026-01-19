using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Currencies;

namespace Main.Abstractions.Dtos.Amw.Sales;

public class SaleDto
{
    public string Id { get; set; } = null!;
    public UserDto Buyer { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime SaleDatetime { get; set; }
    public CurrencyDto Currency { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
    public decimal TotalSum { get; set; }
    public string Storage { get; set; } = null!;
}