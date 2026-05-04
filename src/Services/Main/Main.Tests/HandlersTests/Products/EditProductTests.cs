using Abstractions.Models;
using FluentAssertions;
using FluentValidation;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.PatchProduct;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public class EditProductTests : TestBase
{
    public EditProductTests(CombinedContainerFixture fixture) : base(fixture)
    {
        ProductTestContext.Register(this);
    }
    
    private ProductTestContext TestContext => GetContext<ProductTestContext>();
    
    [Fact]
    public async Task EditArticle_NumberAndName_Succeeds()
    {
        var id = GetFirstId();
        var command = new PatchProductCommand(id,
            new PatchProductDto
            {
                Sku = new PatchField<string> { IsSet = true, Value = "67890" },
                Name = new PatchField<string> { IsSet = true, Value = "Updated Article" }
            });

        var act = () => TestContext.Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var updatedProduct = await GetProduct(id);
        updatedProduct.Should().NotBeNull();
        
        updatedProduct.Name.Should().Be("Updated Article");
        updatedProduct.Sku.Value.Should().Be("67890");
    }

    [Fact]
    public async Task EditArticle_WithInvalidArticleId_FailsValidation()
    {
        var command = new PatchProductCommand(
            999,
            new PatchProductDto
            {
                Sku = new PatchField<string> { IsSet = true, Value = "67890" }
            });
        
        var act = () => TestContext.Mediator.Send(command);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task EditArticle_WithEmptyArticleNumber_FailsValidation()
    {
        var command = new PatchProductCommand(
            GetFirstId(),
            new PatchProductDto
            {
                Sku = new PatchField<string> { IsSet = true, Value = "" }
            });

        var act = () => TestContext.Mediator.Send(command);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task EditArticle_WhenNothingEdited_Succeeds()
    {
        var command = new PatchProductCommand(
            GetFirstId(),
            new PatchProductDto
            {
                Sku = new PatchField<string> { IsSet = false, Value = null }
            });

        var act = () => TestContext.Mediator.Send(command);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EditArticle_WhenArticleNumberNull_FailsValidation()
    {
        var command = new PatchProductCommand(
            GetFirstId(),
            new PatchProductDto
            {
                Sku = new PatchField<string> { IsSet = true, Value = null }
            });
        await Assert.ThrowsAsync<ValidationException>(async () => await TestContext.Mediator.Send(command));
    }

    private int GetFirstId() => TestContext.Products[0].Id;

    private async Task<Product?> GetProduct(int productId)
        => await TestContext.DbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
}