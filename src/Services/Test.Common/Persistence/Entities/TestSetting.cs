using Domain.CommonEntities;
using Domain.Interfaces;

namespace Tests.Persistence.Entities;

internal sealed class TestSetting :
    Setting<TestSettingData>,
    ISetting<TestSetting>
{
    public TestSetting(string json) : base(SettingName, json) { }

    public TestSetting(TestSettingData data) : base(SettingName, data) { }

    public static string SettingName => "TestSetting";
    public static TestSetting Default => new(new TestSettingData());
}

internal sealed record TestSettingData
{
    public int Value { get; init; }
}
