using Domain;

namespace Main.Entities.Currency;

public class CurrencyRateHistory : AuditableEntity<CurrencyRateHistory, int>
{
    public int Id { get; private set; }

    public int FromCurrencyId { get; private set; }
    public int ToCurrencyId { get; private set; }

    public decimal PrevRate { get; private set; }
    public decimal NewRate { get; private set; }

    public CurrencyRate CurrencyRate { get; private set; } = null!;

    private CurrencyRateHistory() {}

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

    public static CurrencyRateHistory Create(
        int from,
        int to,
        decimal prev,
        decimal next)
        => new(from, to, prev, next);

    public override int GetId() => Id;
}