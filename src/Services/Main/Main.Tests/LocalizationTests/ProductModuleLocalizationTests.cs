using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities.Product;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;

namespace Tests.LocalizationTests;

public class ProductModuleLocalizationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void ProductEntityMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(ArticleNotFoundMessage.Instance, out var article).Should().BeTrue();
		article.Should().NotBeNullOrWhiteSpace();
		localizer.TryGet(ProducerNotFoundMessage.Instance, out var producer).Should().BeTrue();
		producer.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void ProductLrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtProducerImportNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}
}
