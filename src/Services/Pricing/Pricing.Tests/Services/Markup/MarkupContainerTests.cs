using FluentAssertions;
using Pricing.Application.Services.Markup;
using Pricing.Entities;
using MarkupModel = Pricing.Application.Models.Markup;

namespace Pricing.Integration.Tests.Services.Markup;

public class MarkupContainerTests
{
    [Fact]
    public void Initialize_WithRangesInDifferentOrder_GeneratesSameVersion()
    {
        var first = CreateContainer(
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [3] = CurrencyRanges(3),
                [2] = CurrencyRanges(2)
            });
        var second = CreateContainer(
            DefaultRanges().Reverse(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] = CurrencyRanges(2).Reverse(),
                [3] = CurrencyRanges(3).Reverse()
            });

        second.CurrentVersion.Should().Be(first.CurrentVersion);
    }

    [Fact]
    public void Initialize_WhenDefaultRangeChanges_GeneratesDifferentVersion()
    {
        var initial = CreateContainer(DefaultRanges());
        var changed = CreateContainer(
        [
            Range(0m, 100m, 0.25m),
            Range(100m, 200m, 0.1m)
        ]);

        changed.CurrentVersion.Should().NotBe(initial.CurrentVersion);
    }

    [Fact]
    public void Initialize_WhenOtherCurrencyRangeChanges_GeneratesDifferentVersion()
    {
        var initial = CreateContainer(
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] = CurrencyRanges(2)
            });
        var changed = CreateContainer(
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] =
                [
                    Range(0m, 200m, 0.2m),
                    Range(200m, 400m, 0.15m)
                ]
            });

        changed.CurrentVersion.Should().NotBe(initial.CurrentVersion);
    }

    [Fact]
    public void Initialize_WhenOtherCurrencyIsAddedOrRemoved_GeneratesDifferentVersion()
    {
        var withoutOtherCurrency = CreateContainer(DefaultRanges());
        var withOtherCurrency = CreateContainer(
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] = CurrencyRanges(2)
            });

        withOtherCurrency.CurrentVersion.Should().NotBe(withoutOtherCurrency.CurrentVersion);
    }

    [Fact]
    public void Initialize_WhenDefaultMarkupChanges_GeneratesDifferentVersion()
    {
        var initial = CreateContainer(DefaultRanges(), defaultMarkup: 0.2m);
        var changed = CreateContainer(DefaultRanges(), defaultMarkup: 0.3m);

        changed.CurrentVersion.Should().NotBe(initial.CurrentVersion);
    }

    [Fact]
    public void Initialize_WithSameDataRepeatedly_KeepsVersion()
    {
        var container = CreateContainer(
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] = CurrencyRanges(2)
            });
        var initialVersion = container.CurrentVersion;

        container.Initialize(
            1,
            new MarkupModel(0.2m),
            DefaultRanges(),
            new Dictionary<int, IEnumerable<MarkupRange>>
            {
                [2] = CurrencyRanges(2)
            });

        container.CurrentVersion.Should().Be(initialVersion);
    }

    private static MarkupContainer CreateContainer(
        IEnumerable<MarkupRange> defaultRanges,
        Dictionary<int, IEnumerable<MarkupRange>>? otherRanges = null,
        decimal defaultMarkup = 0.2m)
    {
        var container = new MarkupContainer();
        container.Initialize(
            1,
            new MarkupModel(defaultMarkup),
            defaultRanges,
            otherRanges ?? []);
        return container;
    }

    private static IReadOnlyCollection<MarkupRange> DefaultRanges() =>
    [
        Range(0m, 100m, 0.2m),
        Range(100m, 200m, 0.1m)
    ];

    private static IReadOnlyCollection<MarkupRange> CurrencyRanges(int multiplier) =>
    [
        Range(0m, 100m * multiplier, 0.2m),
        Range(100m * multiplier, 200m * multiplier, 0.1m)
    ];

    private static MarkupRange Range(decimal start, decimal end, decimal markup)
        => MarkupRange.Create(start, end, markup);
}
