using FluentAssertions;
using CurrencyDomain = Main.Entities.Currency.Currency;

namespace Main.Tests.Domain.Currency;

public class CurrencyTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var currency = CurrencyDomain.Create("US Dollar", "USD", "$", "USD");

        currency.Name.Should().Be("US Dollar");
        currency.ShortName.Should().Be("USD");
        currency.CurrencySign.Should().Be("$");
        currency.Code.Should().Be("USD");
    }
}