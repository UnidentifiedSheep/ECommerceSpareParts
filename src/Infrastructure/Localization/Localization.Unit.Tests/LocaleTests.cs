using FluentAssertions;
using Localization.Abstractions.Models;

namespace Localization.Unit.Tests;

public class LocaleTests
{
    [Theory]
    [InlineData("ru-RU", "RU")]
    [InlineData("ru_ru", "RU")]
    [InlineData("ru", "RU")]
    [InlineData("en-US", "EN")]
    [InlineData("EN", "EN")]
    [InlineData("  de-DE  ", "DE")]
    public void Constructor_NormalizesLocale(string input, string expected)
    {
        Locale locale = input;

        Assert.Equal(expected, locale.ToString());
        Assert.Equal(expected, (string)locale);
    }

    [Fact]
    public void Equality_Operator_ShouldWork()
    {
        Locale l1 = "ru-RU";
        Locale l2 = "ru";
        Locale l3 = "en-US";

        l1.Should().Be(l2);
        l1.Should().NotBe(l3);
        l2.Should().NotBe(l3);
    }

    [Fact]
    public void Equals_Method_ShouldWork()
    {
        Locale l1 = "ru-RU";
        Locale l2 = "ru";
        Locale l3 = "en";

        Assert.True(l1.Equals(l2));
        Assert.False(l1.Equals(l3));
        Assert.False(l1.Equals("string object"));
    }

    [Fact]
    public void GetHashCode_ShouldBeSame_ForSameBaseLocale()
    {
        Locale l1 = "ru-RU";
        Locale l2 = "ru";

        Assert.Equal(l1.GetHashCode(), l2.GetHashCode());
    }

    [Fact]
    public void ImplicitConversion_ShouldWork()
    {
        Locale l = "ru-RU";
        string s = l;

        Assert.Equal("RU", s);
        Assert.Equal("RU", l.ToString());
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_OnInvalidInput(string invalid)
    {
        Assert.Throws<ArgumentNullException>(() => new Locale(invalid));
    }
}