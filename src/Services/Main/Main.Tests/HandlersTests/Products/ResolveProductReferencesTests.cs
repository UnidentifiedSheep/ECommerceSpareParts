using Enums;
using FluentAssertions;
using Main.Application.Dtos.Product.References;
using Main.Application.Handlers.Products;
using Main.Entities.Product.ValueObjects;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public sealed class ResolveProductReferencesTests : IntegrationTest
{
	public ResolveProductReferencesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Resolve_NormalizesSkuAndProducerName_WithOptionalSupplier(bool withSupplier)
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var product = await new ProductBuilder(Faker).WithProducerId(producer.Id).WithSku(new Sku("AB-123"))
			.BuildAndAddToDb(Context, CancellationToken);
		var reference = new ProductReferenceDto
		{
			Sku = " ab 123 ", ProducerName = $" {producer.Name.ToLowerInvariant()} ",
			Supplier = withSupplier ? Supplier.Armtek : null
		};

		var result = await Mediator.Send(new ResolveProductReferencesQuery([reference]), CancellationToken);

		var resolved = result.Products.Should().ContainSingle().Which;
		resolved.ProductId.Should().Be(product.Id);
		resolved.ProducerId.Should().Be(producer.Id);
		resolved.Sku.Should().Be(reference.Sku);
		resolved.ProducerName.Should().Be(reference.ProducerName);
		resolved.Supplier.Should().Be(reference.Supplier);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Resolve_UsesAlias_WhenSupplierMappingIsAbsent(bool withSupplier)
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var product = await new ProductBuilder(Faker).WithProducerId(producer.Id)
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProducerAliasBuilder(Faker).WithProducerId(producer.Id).WithAlias("Producer reference alias")
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await Mediator.Send(new ResolveProductReferencesQuery([new ProductReferenceDto
		{
			Sku = product.Sku.Value, ProducerName = " producer reference ALIAS ",
			Supplier = withSupplier ? Supplier.Armtek : null
		}]), CancellationToken);

		result.Products.Should().ContainSingle().Which.ProductId.Should().Be(product.Id);
	}

	[Fact]
	public async Task Resolve_SupplierMappingOverridesDirectName_OnlyForMatchingSupplier()
	{
		var producers = GetContext<ProducerTestContext>().Producers;
		var direct = await new ProductBuilder(Faker).WithProducerId(producers[0].Id).WithSku(new Sku("SHARED-123"))
			.BuildAndAddToDb(Context, CancellationToken);
		var mapped = await new ProductBuilder(Faker).WithProducerId(producers[1].Id).WithSku(new Sku("SHARED-123"))
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProducerSupplierMappingBuilder(Faker).WithProducerId(producers[1].Id)
			.WithSupplier(Supplier.Armtek).WithSupplierProducerName(producers[0].Name)
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await Mediator.Send(new ResolveProductReferencesQuery(
			new Supplier?[] { Supplier.Armtek, Supplier.Tmtr, null }.Select(supplier => new ProductReferenceDto
			{
				Sku = "shared123", ProducerName = producers[0].Name, Supplier = supplier
			}).ToArray()), CancellationToken);

		result.Products.Select(product => product.ProductId).Should().Equal(mapped.Id, direct.Id, direct.Id);
		result.Products.Select(product => product.ProducerId).Should().Equal(producers[1].Id, producers[0].Id, producers[0].Id);
	}

	[Fact]
	public async Task Resolve_MixedSuppliersAndUnspecifiedSupplier_UseIndependentMappings()
	{
		var producers = GetContext<ProducerTestContext>().Producers;
		var first = await new ProductBuilder(Faker).WithProducerId(producers[0].Id).WithSku(new Sku("SHARED-456"))
			.BuildAndAddToDb(Context, CancellationToken);
		var second = await new ProductBuilder(Faker).WithProducerId(producers[1].Id).WithSku(new Sku("SHARED-456"))
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProducerSupplierMappingBuilder(Faker).WithProducerId(producers[0].Id)
			.WithSupplier(Supplier.Armtek).WithSupplierProducerName("External brand")
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProducerSupplierMappingBuilder(Faker).WithProducerId(producers[1].Id)
			.WithSupplier(Supplier.Tmtr).WithSupplierProducerName("External brand")
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await Mediator.Send(new ResolveProductReferencesQuery(
			new Supplier?[] { Supplier.Armtek, Supplier.Tmtr, null }.Select(supplier => new ProductReferenceDto
			{
				Sku = "shared456", ProducerName = "EXTERNAL BRAND", Supplier = supplier
			}).ToArray()), CancellationToken);

		result.Products.Select(product => product.ProductId).Should().Equal(first.Id, second.Id, null);
		result.Products.Select(product => product.ProducerId).Should().Equal(producers[0].Id, producers[1].Id, null);
	}

	[Fact]
	public async Task Resolve_DirectNameTakesPriorityOverAnotherProducersAlias()
	{
		var producers = GetContext<ProducerTestContext>().Producers;
		var product = await new ProductBuilder(Faker).WithProducerId(producers[0].Id)
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProducerAliasBuilder(Faker).WithProducerId(producers[1].Id).WithAlias(producers[0].Name)
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await Mediator.Send(new ResolveProductReferencesQuery([new ProductReferenceDto
		{
			Sku = product.Sku.Value, ProducerName = producers[0].Name
		}]), CancellationToken);

		result.Products.Should().ContainSingle().Which.ProductId.Should().Be(product.Id);
	}

	[Fact]
	public async Task Resolve_PreservesOrderDuplicatesAndUnresolvedReferences()
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var product = await new ProductBuilder(Faker).WithProducerId(producer.Id)
			.BuildAndAddToDb(Context, CancellationToken);
		var found = new ProductReferenceDto { Sku = product.Sku.Value, ProducerName = producer.Name };
		ProductReferenceDto[] references =
		[
			found,
			found with { ProducerName = "Unknown reference producer" },
			found with { Sku = "UNMATCHED-REFERENCE-SKU" },
			found,
			found with { Sku = " " },
			found with { ProducerName = " " },
			found with { Sku = "---" }
		];

		var result = await Mediator.Send(new ResolveProductReferencesQuery(references), CancellationToken);

		result.Products.Select(item => item.ProductId).Should().Equal(product.Id, null, null, product.Id, null, null, null);
		result.Products.Select(item => item.ProducerId).Should().Equal(producer.Id, null, producer.Id, producer.Id, producer.Id, null, producer.Id);
		result.Products.Select(item => (item.Sku, item.ProducerName, item.Supplier))
			.Should().Equal(references.Select(item => (item.Sku, item.ProducerName, item.Supplier)));
	}

	[Fact]
	public async Task Resolve_DoesNotMatchSkuAndProducerFromDifferentReferences()
	{
		var producers = GetContext<ProducerTestContext>().Producers;
		await new ProductBuilder(Faker).WithProducerId(producers[0].Id).WithSku(new Sku("CROSS-AAA"))
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProductBuilder(Faker).WithProducerId(producers[1].Id).WithSku(new Sku("CROSS-BBB"))
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await Mediator.Send(new ResolveProductReferencesQuery([
			new ProductReferenceDto { Sku = "crossaaa", ProducerName = producers[1].Name },
			new ProductReferenceDto { Sku = "crossbbb", ProducerName = producers[0].Name }
		]), CancellationToken);

		result.Products.Should().HaveCount(2).And.OnlyContain(product => product.ProductId == null);
	}

	[Fact]
	public async Task Resolve_EmptyReferences_ReturnsEmptyResult()
	{
		var result = await Mediator.Send(new ResolveProductReferencesQuery([]), CancellationToken);

		result.Products.Should().BeEmpty();
	}

	[Theory]
	[InlineData("direct")]
	[InlineData("alias")]
	[InlineData("supplier")]
	public async Task Resolve_ProducerExistsWithoutProduct_ReturnsProducerId(string source)
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var producerName = producer.Name;
		Supplier? supplier = null;
		if (source == "alias")
		{
			producerName = "Unmatched product brand alias";
			await new ProducerAliasBuilder(Faker).WithProducerId(producer.Id).WithAlias(producerName)
				.BuildAndAddToDb(Context, CancellationToken);
		}
		else if (source == "supplier")
		{
			producerName = "Unmatched product supplier brand";
			supplier = Supplier.Armtek;
			await new ProducerSupplierMappingBuilder(Faker).WithProducerId(producer.Id)
				.WithSupplier(supplier.Value).WithSupplierProducerName(producerName)
				.BuildAndAddToDb(Context, CancellationToken);
		}

		var result = await Mediator.Send(new ResolveProductReferencesQuery([new ProductReferenceDto
		{
			Sku = "UNKNOWN-PRODUCT-SKU", ProducerName = producerName, Supplier = supplier
		}]), CancellationToken);

		var resolved = result.Products.Should().ContainSingle().Which;
		resolved.ProducerId.Should().Be(producer.Id);
		resolved.ProductId.Should().BeNull();
	}

}
