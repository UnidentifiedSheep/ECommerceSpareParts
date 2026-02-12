namespace Contracts.Models.Currency;

public class Currency
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CurrencySign { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal ToUsdRate { get; set; }
}