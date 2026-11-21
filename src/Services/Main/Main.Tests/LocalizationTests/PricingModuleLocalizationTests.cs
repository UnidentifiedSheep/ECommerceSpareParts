using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Entities;

namespace Tests.LocalizationTests;

public class PricingModuleLocalizationTests : ServiceTest
{
	[Fact]
	public void MarkupMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(MarkupGroupNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void ResyncLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtResyncOurWarehouseOffersNameMessage.Instance, out var name).Should().BeTrue();
		name.Should().NotBeNullOrWhiteSpace();
		localizer.TryGet(LrtResyncOurWarehouseOffersDescriptionMessage.Instance, out var description)
			.Should()
			.BeTrue();
		description.Should().NotBeNullOrWhiteSpace();
	}
}
