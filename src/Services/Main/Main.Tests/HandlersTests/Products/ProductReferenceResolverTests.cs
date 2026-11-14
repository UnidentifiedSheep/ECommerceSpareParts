using Enums;
using FluentAssertions;
using Main.Entities.Product.ValueObjects;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public sealed class ProductReferenceResolverTests : IntegrationTest
{
	public ProductReferenceResolverTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProducerTestContext>();

	[Fact]
	public async Task Resolve_ReturnsProductAndProducerIds()
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var product = await new ProductBuilder(Faker).WithProducerId(producer.Id)
			.WithSku(new Sku("AB-123"))
			.BuildAndAddToDb(Context, CancellationToken);
		var reference = new ProductReference(" ab 123 ", $" {producer.Name.ToLowerInvariant()} ", null);

		var result = await Resolver().ResolveAsync([reference], CancellationToken);

		var resolved = result.Should().ContainSingle().Which;
		resolved.ProductId.Should().Be(product.Id);
		resolved.ProducerId.Should().Be(producer.Id);
		resolved.Sku.Should().Be(reference.Sku);
		resolved.ProducerName.Should().Be(reference.ProducerName);
	}

	[Fact]
	public async Task Resolve_WithoutReferences_ReturnsEmpty()
		=> (await Resolver().ResolveAsync([], CancellationToken)).Should().BeEmpty();

	[Fact]
	public async Task GetSupplierReferences_WithoutIds_ReturnsEmpty()
		=> (await Resolver().GetSupplierReferencesAsync([], Supplier.Armtek, CancellationToken)).Should().BeEmpty();

	private IProductReferenceResolver Resolver()
		=> Scope.ServiceProvider.GetRequiredService<IProductReferenceResolver>();
}
