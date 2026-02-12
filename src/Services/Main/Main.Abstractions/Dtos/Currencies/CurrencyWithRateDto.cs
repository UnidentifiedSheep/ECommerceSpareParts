namespace Main.Abstractions.Dtos.Currencies;

public class CurrencyWithRateDto
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CurrencySign { get; set; } = null!;

    public string Code { get; set; } = null!;
    
    public decimal ToUsdRate { get; set; }
}