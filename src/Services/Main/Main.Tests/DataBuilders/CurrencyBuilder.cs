using Bogus;
using Main.Entities.Currency;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class CurrencyBuilder(Faker faker) : BuilderBase<Currency>(faker)
{
    public string? Name { get; private set; }
    public string? ShortName { get; private set; }
    public string? Code { get; private set; }
    public string? Sign { get; private set; }

    public CurrencyBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public CurrencyBuilder WithShortName(string shortName)
    {
        ShortName = shortName;
        return this;
    }

    public CurrencyBuilder WithCode(string code)
    {
        Code = code;
        return this;
    }

    public CurrencyBuilder WithSign(string sign)
    {
        Sign = sign;
        return this;
    }
    
    public override Currency Build()
    {
        var rndCurrency = Faker.Finance.Currency(true);
        return Currency.Create(
            Name ?? rndCurrency.Description,
            ShortName ?? rndCurrency.Description[..3],
            Sign ?? Faker.Lorem.Letter(3),
            Code ?? rndCurrency.Code);
    }
}