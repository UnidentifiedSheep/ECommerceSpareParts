using Domain;
using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.LocalizationTests;

public class StorageModuleLocalizationTests : ServiceTest
{
	[Fact]
	public void StorageEntityMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(StorageContentNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void StorageSettingMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(StorageContentSettingExtractionPolicyNameMessage.Instance, out var text)
			.Should()
			.BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void SharedValidationMessages_AreResolvedFromDomainResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(PriceMustBePositiveMessage.Instance, out var price).Should().BeTrue();
		price.Should().NotBeNullOrWhiteSpace();
		localizer.TryGet(PositionCountMustBeGreaterThanZeroMessage.Instance, out var count).Should().BeTrue();
		count.Should().NotBeNullOrWhiteSpace();
	}
}
