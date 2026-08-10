using Bogus;
using Tests.Abstractions;
using Tests.Persistence.Entities;

namespace Tests.DataBuilders.Settings;

internal sealed class TestSettingBuilder(Faker faker)
    : BuilderBase<TestSetting>(faker)
{
    public int? Value { get; private set; }

    public TestSettingBuilder WithValue(int value)
    {
        Value = value;
        return this;
    }

    public override TestSetting Build()
    {
        return new TestSetting(
            new TestSettingData
            {
                Value = Value ?? Faker.Random.Int(1, 10_000)
            });
    }
}
