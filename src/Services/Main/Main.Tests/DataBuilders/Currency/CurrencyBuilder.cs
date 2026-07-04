using Bogus;
using Tests.Abstractions;

namespace Tests.DataBuilders.Currency;

public class CurrencyBuilder(Faker faker) : BuilderBase<Main.Entities.Currency.Currency>(faker)
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

    public override Main.Entities.Currency.Currency Build()
    {
        return Main.Entities.Currency.Currency.Create(
            Name ?? Faker.Lorem.Letter(24),
            ShortName ?? Faker.Lorem.Letter(5),
            Sign ?? Faker.Lorem.Letter(3),
            Code ?? Faker.Lorem.Letter(6));
    }

    public override IReadOnlyCollection<Main.Entities.Currency.Currency> BuildMany(int count)
    {
        return Enumerable.Range(0, count)
            .Select(index => Main.Entities.Currency.Currency.Create(
                Name ?? $"{Faker.Lorem.Letter(20)}-{index}",
                ShortName ?? $"{Faker.Lorem.Letter(3)}-{index}",
                Sign ?? $"{Faker.Lorem.Letter()}-{index}",
                Code ?? $"{Faker.Lorem.Letter(4)}-{index}"))
            .ToList();
    }
}