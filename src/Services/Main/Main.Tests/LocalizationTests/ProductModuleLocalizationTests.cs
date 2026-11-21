using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities.Product;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.LocalizationTests;

public class ProductModuleLocalizationTests : ServiceTest
{
	[Fact]
	public void ProductEntityMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(ArticleNotFoundMessage.Instance, out var article).Should().BeTrue();
		article.Should().NotBeNullOrWhiteSpace();
		localizer.TryGet(ProducerNotFoundMessage.Instance, out var producer).Should().BeTrue();
		producer.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void ProductLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtProducerImportNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}
}
