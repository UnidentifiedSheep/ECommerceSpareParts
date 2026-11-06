using Main.Entities;
using Tests.Abstractions.Test;
using FluentValidation.TestHelper;
using Exceptions;
using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Products.Commands.CreateProducts;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
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
	public async Task CreateProducts_WithExistingProduct_Throws()
	{
		var product = CreateDtos(1)[0];
		await Mediator.Send(new CreateProductsCommand([product]), CancellationToken);

		var act = () => Mediator.Send(new CreateProductsCommand([product]));

		await act.Should().ThrowAsync<InvalidInputException>();
	}

	[Fact]
	public async Task CreateProducts_WithDuplicateInsideBatch_Throws()
	{
		var product = CreateDtos(1)[0];

		var act = () => Mediator.Send(
			new CreateProductsCommand(
			[
				product,
				product with
				{
					Name = TestContext.Faker.Commerce.ProductName()
				}
			]));

		await act.Should().ThrowAsync<InvalidInputException>();
	}

	private void Validate(CreateProductDto product, Product dbProduct)
	{
		dbProduct.Sku.Value.Should().Be(product.Sku);
		dbProduct.Name.Value.Should().Be(product.Name);
		dbProduct.ProducerId.Should().Be(product.ProducerId);
	}

	private async Task<List<Product>> GetProducts() => await TestContext.DbContext.Products.ToListAsync();

	private List<CreateProductDto> CreateDtos(int count)
	{
		return Enumerable
			.Range(1, count)
			.Select(_ => new CreateProductDto
			{
				Sku = TestContext.Faker.Lorem.Letter(30),
				Name = TestContext.Faker.Commerce.ProductName(),
				ProducerId = TestContext.Faker.PickRandom(TestContext.Producers.Select(p => p.Id).ToArray())
			})
			.ToList();
	}
}

public class CreateProductsValidationTests : ValidationTest<CreateProductsValidation, CreateProductsCommand>
{
	private static CreateProductDto ValidProduct() => new() { Sku = "ABC-123", Name = "Product", ProducerId = 1 };

	[Fact]
	public async Task CreateArticle_WithEmptyList_FailsValidation()
	{
		var result = await ValidateAsync(new CreateProductsCommand([]));
		result.ShouldHaveValidationErrorFor(x => x.NewProducts)
			.WithErrorCode(ArticleCreateArticlesMustHaveAtLeastOneMessage.Instance.MessageKey).WithCustomState(ArticleCreateArticlesMustHaveAtLeastOneMessage.Instance).Only();
	}

	[Fact]
	public async Task CreateArticle_WithLongName_FailsValidation()
	{
		var result = await ValidateAsync(new CreateProductsCommand([ValidProduct() with { Name = new string('x', 256) }]));
		result.ShouldHaveValidationErrorFor("NewProducts[0].Name")
			.WithErrorCode(ArticleNameMaxLength255Message.Instance.MessageKey).WithCustomState(ArticleNameMaxLength255Message.Instance).Only();
	}

	[Fact]
	public async Task CreateArticle_WithManyItems_FailsValidation()
	{
		var result = await ValidateAsync(new CreateProductsCommand(Enumerable.Range(0, 200).Select(_ => ValidProduct()).ToList()));
		result.ShouldHaveValidationErrorFor(x => x.NewProducts)
			.WithErrorCode(ArticleCreateArticlesMax100AtOnceMessage.Instance.MessageKey).WithCustomState(ArticleCreateArticlesMax100AtOnceMessage.Instance).Only();
	}
}
