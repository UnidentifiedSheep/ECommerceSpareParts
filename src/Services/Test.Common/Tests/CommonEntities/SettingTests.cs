using Domain.CommonEntities;
using Domain.CommonEntities.Events;
using FluentAssertions;

namespace Tests.Tests.CommonEntities;

public sealed class SettingTests
{
    [Fact]
    public void OnCreated_RaisesUpdatedEventWithInitialValue()
    {
        var setting = new TestSetting(new TestSettingData(1));

        setting.OnCreated();

        var @event = setting.FlushDomainEvents()
            .OfType<SettingUpdatedDomainEvent>()
            .Should().ContainSingle()
            .Which;
        @event.Key.Should().Be(TestSetting.Name);
        @event.Value.Should().Be("{\"Value\":1}");
        @event.ChangedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetData_RaisesUpdatedEventWithNewValue()
    {
        var setting = new TestSetting(new TestSettingData(1));

        setting.SetData(new TestSettingData(2));

        var @event = setting.FlushDomainEvents()
            .OfType<SettingUpdatedDomainEvent>()
            .Should().ContainSingle()
            .Which;
        @event.Key.Should().Be(TestSetting.Name);
        @event.Value.Should().Be("{\"Value\":2}");
    }

    [Fact]
    public void SetData_MultipleTimesBeforeFlush_KeepsLatestEvent()
    {
        var setting = new TestSetting(new TestSettingData(1));

        setting.SetData(new TestSettingData(2));
        setting.SetData(new TestSettingData(3));

        setting.FlushDomainEvents()
            .OfType<SettingUpdatedDomainEvent>()
            .Should().ContainSingle()
            .Which.Value.Should().Be("{\"Value\":3}");
    }

    private sealed record TestSettingData(int Value);

    private sealed class TestSetting(TestSettingData data)
        : Setting<TestSettingData>(Name, data)
    {
        public const string Name = "test-setting";
    }
}
