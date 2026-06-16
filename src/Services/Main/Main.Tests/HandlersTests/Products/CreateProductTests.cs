using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Main.Enums.Products;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public class CreateProductTests : IntegrationTest
{
    public CreateProductTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task CreateManyArticles_Succeeds()
    {
        var dtos = CreateDtos(10);

        var command = new CreateProductsCommand(dtos);

        var act = () => Mediator.Send(command);
        await act.Should().NotThrowAsync();

        var products = await GetProducts();

        products.Should().HaveCount(dtos.Count);

        for (var i = 0; i < dtos.Count; i++)
            Validate(dtos[i], products[i]);
    }

    [Fact]
    public async Task CreateArticle_WithEmptyList_FailsValidation()
    {
        var command = new CreateProductsCommand([]);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreateArticle_WithLongName_FailsValidation()
    {
        var dtos = CreateDtos(1)[0] with
        {
            Name = string.Join(" ", TestContext.Faker.Lorem.Words(100))
        };

        var command = new CreateProductsCommand([dtos]);
        var act = () => Mediator.Send(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateArticle_WithManyItems_FailsValidation()
    {
        var act = () => Mediator.Send(new CreateProductsCommand(CreateDtos(200)));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateArticle_WithInvalidProducer_ThrowsProducerNotFoundException()
    {
        var dto = CreateDtos(1)[0] with
        {
            ProducerId = int.MaxValue
        };

        var act = () => Mediator.Send(new CreateProductsCommand([dto]));

        await act.Should().ThrowAsync<DbValidationException>();
    }

    [Fact]
    public async Task CreateProducts_WithSkipExisting_SkipsExistingProducts()
    {
        var existing = CreateDtos(1)[0];
        var newProduct = CreateDtos(1)[0];

        await Mediator.Send(new CreateProductsCommand([existing]));

        var result = await Mediator.Send(new CreateProductsCommand(
            [existing, newProduct],
            CreateProductsConflictPolicy.SkipExisting));

        result.CreatedIds.Should().HaveCount(1);
        result.Skipped.Should().Be(1);

        var products = await GetProducts();
        products.Should().HaveCount(2);
        products.Should().ContainSingle(x =>
            x.Sku.NormalizedValue == new Sku(existing.Sku).NormalizedValue &&
            x.ProducerId == existing.ProducerId);
        products.Should().ContainSingle(x =>
            x.Sku.NormalizedValue == new Sku(newProduct.Sku).NormalizedValue &&
            x.ProducerId == newProduct.ProducerId);
    }

    [Fact]
    public async Task CreateProducts_WithSkipExisting_SkipsDuplicateProductsInBatch()
    {
        var product = CreateDtos(1)[0];

        var result = await Mediator.Send(new CreateProductsCommand(
            [product, product with { Name = TestContext.Faker.Commerce.ProductName() }],
            CreateProductsConflictPolicy.SkipExisting));

        result.CreatedIds.Should().HaveCount(1);
        result.Skipped.Should().Be(1);

        var products = await GetProducts();
        products.Should().ContainSingle(x =>
            x.Sku.NormalizedValue == new Sku(product.Sku).NormalizedValue &&
            x.ProducerId == product.ProducerId);
    }

    [Fact]
    public async Task CreateProducts_WithSkipExisting_DoesNotSkipSameSkuForAnotherProducer()
    {
        var producerIds = TestContext.Producers
            .Take(2)
            .Select(x => x.Id)
            .ToArray();

        var first = CreateDtos(1)[0] with
        {
            ProducerId = producerIds[0]
        };
        var second = first with
        {
            ProducerId = producerIds[1]
        };

        await Mediator.Send(new CreateProductsCommand([first]));

        var result = await Mediator.Send(new CreateProductsCommand(
            [second],
            CreateProductsConflictPolicy.SkipExisting));

        result.CreatedIds.Should().HaveCount(1);
        result.Skipped.Should().Be(0);

        var products = await GetProducts();
        products.Should().HaveCount(2);
        products.Should().OnlyContain(x => x.Sku.Value == first.Sku);
    }

    private void Validate(CreateProductDto product, Product dbProduct)
    {
        dbProduct.Sku.Value.Should().Be(product.Sku);
        dbProduct.Name.Value.Should().Be(product.Name);
        dbProduct.ProducerId.Should().Be(product.ProducerId);
    }

    private async Task<List<Product>> GetProducts()
    {
        return await TestContext.DbContext.Products.ToListAsync();
    }

    private List<CreateProductDto> CreateDtos(int count)
    {
        return Enumerable.Range(1, count).Select(_ => new CreateProductDto
        {
            Sku = TestContext.Faker.Lorem.Letter(30),
            Name = TestContext.Faker.Commerce.ProductName(),
            ProducerId = TestContext.Faker.PickRandom(TestContext.Producers.Select(p => p.Id).ToArray())
        }).ToList();
    }
}
