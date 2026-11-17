using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities.Currency;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;

namespace Tests.LocalizationTests;

public class CurrencyModuleLocalizationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void CurrencyEntityMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(CurrencyNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void CurrencyLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtCurrencyRatesUpdateNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}
}
