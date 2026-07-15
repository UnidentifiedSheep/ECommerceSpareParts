using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Currency;

public class CurrencyRate : AuditableEntity<CurrencyRate, (int, int)>, ILinqEntity<CurrencyRate, (int, int)>
{
    private readonly List<CurrencyRateHistory> _history = [];

    private CurrencyRate() { }

    private CurrencyRate(
        int fromCurrencyId,
        int toCurrencyId,
        decimal rate)
    {
        fromCurrencyId.EnsureNotEqual(
            toCurrencyId,
            () => new InvalidOperationException("Currency rate self reference is not permitted."));

        FromCurrencyId = fromCurrencyId;
        ToCurrencyId = toCurrencyId;
        SetRate(rate);
    }

    public int FromCurrencyId { get; }
    public int ToCurrencyId { get; }

    public decimal Rate { get; private set; }

    public Currency FromCurrency { get; private set; } = null!;
    public Currency ToCurrency { get; private set; } = null!;
    public IReadOnlyCollection<CurrencyRateHistory> History => _history;

    public static Expression<Func<CurrencyRate, (int, int)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.FromCurrencyId, x.ToCurrencyId);
    }

    public static Expression<Func<CurrencyRate, bool>> GetEqualityExpression((int, int) key)
    {
        return x => x.FromCurrencyId == key.Item1 && x.ToCurrencyId == key.Item2;
    }

    public static CurrencyRate Create(
        int fromCurrencyId,
        int toCurrencyId,
        decimal rate)
    {
        return new CurrencyRate(
            fromCurrencyId,
            toCurrencyId,
            rate);
    }

    public void SetRate(decimal rate)
    {
        if (Rate == rate) return;

        var prev = Rate;
        Rate = rate.EnsureGreaterThan(
            0,
            () => new InvalidOperationException("Currency rate must be greater than 0"));

        _history.Add(
            CurrencyRateHistory.Create(
                FromCurrencyId,
                ToCurrencyId,
                prev,
                rate));
    }

    public override (int, int) GetId() { return (FromCurrencyId, ToCurrencyId); }
}