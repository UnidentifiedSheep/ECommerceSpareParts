using BulkValidation.Core.Attributes;
using Domain.Extensions;

namespace Main.Entities.Currency;

public class Currency
{
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

    public virtual ICollection<CurrencyHistory> CurrencyHistories { get; set; } = new List<CurrencyHistory>();

    public virtual CurrencyToUsd? CurrencyToUsd { get; set; }
    
    private Currency() {}

    private Currency(string name, string shortName, string currencySign, string code)
    {
        SetName(name);
        SetShortName(shortName);
        SetCurrencySign(currencySign);
        SetCode(code);
    }

    public static Currency Create(string name, string shortName, string currencySign, string code)
    {
        return new Currency(name, shortName, currencySign, code);
    }

    private void SetName(string name)
    {
        Name = name
            .Trim()
            .AgainstNullOrWhiteSpace("currency.name.not.empty")
            .AgainstTooLong(128, "currency.name.max.length")
            .AgainstTooShort(3, "currency.name.min.length");
    }

    private void SetShortName(string name)
    {
        ShortName = name
            .Trim()
            .AgainstNullOrWhiteSpace("currency.shortName.not.empty")
            .AgainstTooLong(5, "currency.shortName.max.length")
            .AgainstTooShort(2, "currency.shortName.min.length");
    }

    private void SetCurrencySign(string currencySign)
    {
        CurrencySign = currencySign
            .Trim()
            .AgainstNullOrWhiteSpace("currency.sign.not.empty")
            .AgainstTooLong(3, "currency.sign.max.length")
            .AgainstTooShort(1, "currency.sign.min.length");
    }

    private void SetCode(string code)
    {
        Code = code
            .Trim()
            .AgainstNullOrWhiteSpace("currency.code.not.empty")
            .AgainstTooLong(26, "currency.code.max.length")
            .AgainstTooShort(2, "currency.code.min.length");
    }
}