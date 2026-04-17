namespace Main.Entities.Currency;

public class CurrencyHistory
{
    public int Id { get; private set; }

    public int CurrencyId { get; private set; }

    public decimal PrevValue { get; private set; }

    public decimal NewValue { get; private set; }

    public DateTime Datetime { get; private set; }
    
    private CurrencyHistory() {}

    private CurrencyHistory(int currencyId, decimal prevValue, decimal newValue)
    {
        CurrencyId = currencyId;
        PrevValue = prevValue;
        NewValue = newValue;
        Datetime = DateTime.UtcNow;
    }

    internal static CurrencyHistory Create(int currencyId, decimal prevValue, decimal newValue)
    {
        return new CurrencyHistory(currencyId, prevValue, newValue);
    }
}