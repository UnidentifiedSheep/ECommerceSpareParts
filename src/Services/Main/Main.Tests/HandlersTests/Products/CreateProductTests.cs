using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

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
        
        var act = () => TestContext.Mediator.Send(command);
        await act.Should().NotThrowAsync();
        
        var products = await GetProducts();
        
        products.Should().HaveCount(dtos.Count);

        for (int i = 0; i < dtos.Count; i++)
            Validate(dtos[i], products[i]);
    }

    [Fact]
    public async Task CreateArticle_WithEmptyList_FailsValidation()
    {
        var command = new CreateProductsCommand([]);

        await Assert.ThrowsAsync<ValidationException>(() => TestContext.Mediator.Send(command));
    }

    [Fact]
    public async Task CreateArticle_WithLongName_FailsValidation()
    {
        var dtos = CreateDtos(1)[0] with
        {
            Name = string.Join(" ", TestContext.Faker.Lorem.Words(100))
        };
        
        var command = new CreateProductsCommand([dtos]);
        var act = () => TestContext.Mediator.Send(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateArticle_WithManyItems_FailsValidation()
    {
        var act = () => TestContext.Mediator.Send(new CreateProductsCommand(CreateDtos(200)));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateArticle_WithInvalidProducer_ThrowsProducerNotFoundException()
    {
        var dto = CreateDtos(1)[0] with
        {
            ProducerId = int.MaxValue
        };
        
        var act = () => TestContext.Mediator.Send(new CreateProductsCommand([dto]));

        await act.Should().ThrowAsync<DbValidationException>();
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