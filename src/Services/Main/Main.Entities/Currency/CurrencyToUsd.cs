using Domain;

namespace Main.Entities.Currency;

public class CurrencyToUsd : Entity<CurrencyToUsd, int>    
{
    public int CurrencyId { get; private set; }
    public decimal ToUsd { get; private set; }
    
    private CurrencyToUsd() {}

    private CurrencyToUsd(int currencyId, decimal toUsd)
    {
        CurrencyId = currencyId;
        SetToUsd(toUsd);
    }

    internal static CurrencyToUsd Create(int currencyId, decimal toUsd)
    {
        return new CurrencyToUsd(currencyId, toUsd);
    }

    internal void SetToUsd(decimal toUsd)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(toUsd);
        ToUsd = toUsd;
    }
    
    public override int GetId() => CurrencyId;
}