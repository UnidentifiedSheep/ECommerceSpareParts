using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Currency;

public class Currency : Entity<Currency, int>, ILinqEntity<Currency, int>
{
    private readonly List<CurrencyRate> _ratesFrom = [];

    private readonly List<CurrencyRate> _ratesTo = [];

    private Currency() { }

    private Currency(
        string name,
        string shortName,
        string currencySign,
        string code)
    {
        SetName(name);
        SetShortName(shortName);
        SetCurrencySign(currencySign);
        SetCode(code);
    }

    [Validate]
    public int Id { get; private set; }

    [Validate]
    public string ShortName { get; private set; } = null!;

    [Validate]
    public string Name { get; private set; } = null!;

    [Validate]
    public string CurrencySign { get; private set; } = null!;

    [Validate]
    public string Code { get; private set; } = null!;

    public IReadOnlyCollection<CurrencyRate> RatesFrom => _ratesFrom;
    public IReadOnlyCollection<CurrencyRate> RatesTo => _ratesTo;

    public static Expression<Func<Currency, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<Currency, bool>> GetEqualityExpression(int key) { return x => x.Id == key; }

    public static Currency Create(
        string name,
        string shortName,
        string currencySign,
        string code)
    {
        return new Currency(
            name,
            shortName,
            currencySign,
            code);
    }

    private void SetName(string name)
    {
        Name = name
            .Trim()
            .EnsureNotNullOrWhiteSpace("currency.name.not.empty")
            .EnsureMaxLength(128, "currency.name.max.length")
            .EnsureMinLength(3, "currency.name.min.length");
    }

    private void SetShortName(string name)
    {
        ShortName = name
            .Trim()
            .EnsureNotNullOrWhiteSpace("currency.shortName.not.empty")
            .EnsureMaxLength(5, "currency.shortName.max.length")
            .EnsureMinLength(2, "currency.shortName.min.length");
    }

    private void SetCurrencySign(string currencySign)
    {
        CurrencySign = currencySign
            .Trim()
            .EnsureNotNullOrWhiteSpace("currency.sign.not.empty")
            .EnsureMaxLength(3, "currency.sign.max.length")
            .EnsureMinLength(1, "currency.sign.min.length");
    }

    private void SetCode(string code)
    {
        Code = code
            .Trim()
            .EnsureNotNullOrWhiteSpace("currency.code.not.empty")
            .EnsureMaxLength(26, "currency.code.max.length")
            .EnsureMinLength(2, "currency.code.min.length");
    }

    public override int GetId() { return Id; }
}