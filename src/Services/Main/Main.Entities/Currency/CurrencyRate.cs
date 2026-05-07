using Domain;
using Domain.Extensions;

namespace Main.Entities.Currency;

public class CurrencyRate : AuditableEntity<CurrencyRate, (int, int)>
{
    public int FromCurrencyId { get; private set; }
    public int ToCurrencyId { get; private set; }
    
    public decimal Rate { get; private set; }
    
    public Currency FromCurrency { get; private set; } = null!;
    public Currency ToCurrency { get; private set; } = null!;
    
    private readonly List<CurrencyRateHistory> _history = [];
    public IReadOnlyCollection<CurrencyRateHistory> History => _history;
    
    private CurrencyRate() { }

    private CurrencyRate(int fromCurrencyId, int toCurrencyId, decimal rate)
    {
        fromCurrencyId.AgainstEqual(
            next: toCurrencyId,
            exceptionFactory: () => new InvalidOperationException("Currency rate self reference is not permitted."));
        
        FromCurrencyId = fromCurrencyId;
        ToCurrencyId = toCurrencyId;
        SetRate(rate);
    }

    public static CurrencyRate Create(int fromCurrencyId, int toCurrencyId, decimal rate) 
        => new(fromCurrencyId, toCurrencyId, rate);

    public void SetRate(decimal rate)
    {
        if (Rate == rate) return;
        
        var prev = Rate;
        Rate = rate.AgainstLessOrEqual(
            min: 0,
            exceptionFactory: () => new InvalidOperationException("Currency rate must be greater than 0"));

        _history.Add(CurrencyRateHistory.Create(FromCurrencyId, ToCurrencyId, prev, rate));
    }
    
    public override (int, int) GetId() => (FromCurrencyId, ToCurrencyId);
}