using Analytics.Application.NamedObjects.Metrics.MetricInputBases;
using Analytics.Application.NamedObjects.Metrics.MetricInputValidators;
using FluentAssertions;

namespace Analytics.Integration.Tests.Metrics.Validators;

public class MetricInputBaseValidatorTests
{
    private readonly MetricInputBaseValidator _validator = new();

    [Fact]
    public void Validate_WhenRangeStartIsBeforeRangeEnd_ShouldBeValid()
    {
        var input = CreateInput(
            rangeStart: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            rangeEnd: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRangeStartEqualsRangeEnd_ShouldBeValid()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var input = CreateInput(
            rangeStart: date,
            rangeEnd: date);

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRangeStartIsAfterRangeEnd_ShouldBeInvalid()
    {
        var input = CreateInput(
            rangeStart: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            rangeEnd: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
    }

    private static MetricInputBase CreateInput(
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        return new MetricInputBase
        {
            CurrencyId = 1,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };
    }
}
