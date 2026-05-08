using Bogus;
using Main.Entities.Currency;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class CurrencyRateBuilder(Faker faker) : BuilderBase<CurrencyRate>(faker)
{
    public int? FromCurrencyId { get; private set; }
    public int? ToCurrencyId { get; private set; }

    public decimal? Rate { get; private set; }

    public CurrencyRateBuilder WithFromCurrencyId(int fromCurrencyId)
    {
        FromCurrencyId = fromCurrencyId;
        return this;
    }

    public CurrencyRateBuilder WithToCurrencyId(int toCurrencyId)
    {
        ToCurrencyId = toCurrencyId;
        return this;
    }

    public CurrencyRateBuilder WithRate(decimal rate)
    {
        Rate = rate;
        return this;
    }

    public override CurrencyRate Build()
    {
        return CurrencyRate.Create(
            FromCurrencyId ?? Faker.Random.Int(1),
            ToCurrencyId ?? Faker.Random.Int(1),
            Rate ?? Math.Round(Faker.Random.Decimal(1), 2));
    }
}