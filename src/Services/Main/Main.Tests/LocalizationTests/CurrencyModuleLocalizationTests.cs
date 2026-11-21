using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities.Currency;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.LocalizationTests;

public class CurrencyModuleLocalizationTests : ServiceTest
{
	[Fact]
	public void CurrencyEntityMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(CurrencyNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void CurrencyLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtCurrencyRatesUpdateNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}
}
