namespace Main.Entities;

public partial class CurrencyToUsd
{
    public int CurrencyId { get; set; }

    public decimal ToUsd { get; set; }

    public virtual Currency Currency { get; set; } = null!;
}
