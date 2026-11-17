using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Entities;
using Tests.TestContainers.Combined;

namespace Tests.LocalizationTests;

public class PricingModuleLocalizationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void MarkupMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(MarkupGroupNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void ResyncLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtResyncOurWarehouseOffersNameMessage.Instance, out var name).Should().BeTrue();
		name.Should().NotBeNullOrWhiteSpace();
		localizer.TryGet(LrtResyncOurWarehouseOffersDescriptionMessage.Instance, out var description)
			.Should()
			.BeTrue();
		description.Should().NotBeNullOrWhiteSpace();
	}
}
