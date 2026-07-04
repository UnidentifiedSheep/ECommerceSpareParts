using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Currency;

public class CurrencyRateHistory : AuditableEntity<CurrencyRateHistory, int>,
    ILinqEntity<CurrencyRateHistory, int>
{
    private CurrencyRateHistory() { }

    private CurrencyRateHistory(
        int from,
        int to,
        decimal prev,
        decimal next)
    {
        FromCurrencyId = from;
        ToCurrencyId = to;
        PrevRate = prev;
        NewRate = next;
    }

    public int Id { get; private set; }

    public int FromCurrencyId { get; private set; }
    public int ToCurrencyId { get; private set; }

    public decimal PrevRate { get; private set; }
    public decimal NewRate { get; private set; }

    public CurrencyRate CurrencyRate { get; private set; } = null!;

    public static Expression<Func<CurrencyRateHistory, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<CurrencyRateHistory, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static CurrencyRateHistory Create(
        int from,
        int to,
        decimal prev,
        decimal next)
    {
        return new CurrencyRateHistory(
            from,
            to,
            prev,
            next);
    }

    public override int GetId() { return Id; }
}