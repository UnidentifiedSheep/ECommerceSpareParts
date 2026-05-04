using FluentAssertions;
using FluentValidation;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.MakeLinkageBetweenArticles;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Base;

using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Products;

public class MakeLinkageBetweenProductsTests : TestBase
{
    
    public MakeLinkageBetweenProductsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        ProductTestContext.Register(this);
    }
    
    private ProductTestContext TestContext => GetContext<ProductTestContext>();
    
    [Fact]
    public async Task MakeLinkage_SameIds_FailsValidation()
    {
        var newLinkage = new NewProductLinkageDto
        {
            ProductId = 1,
            CrossProductId = 1,
            LinkageType = ProductLinkageType.FullCross
        };
        var command = new MakeLinkageBetweenProductsCommand([newLinkage]);
        await Assert.ThrowsAsync<ValidationException>(
            async () => await TestContext.Mediator.Send(command));
    }

    [Fact]
    public async Task MakeLinkage_SingleCrosses_Succeeds()
    {
        var l = TestContext.Products[0].Id;
        var r = TestContext.Products[1].Id;
        var newLinkage = new NewProductLinkageDto
        {
            ProductId = l,
            CrossProductId = r,
            LinkageType = ProductLinkageType.SingleCross
        };
        var command = new MakeLinkageBetweenProductsCommand([newLinkage]);

        var act = () => TestContext.Mediator.Send(command);
        await act.Should().NotThrowAsync();

        var crosses = await Context.ProductCrosses
            .AsNoTracking()
            .Where(x => x.LeftProductId == l || x.RightProductId == l)
            .FirstOrDefaultAsync();

        crosses.Should().NotBeNull();
    }
    
    [Fact]
    public async Task MakeLinkage_FullCross_CreatesAllCombinations()
    {
        var p1 = TestContext.Products[0].Id;
        var p2 = TestContext.Products[1].Id;
        var p3 = TestContext.Products[2].Id;

        await Context.ProductCrosses.AddAsync(ProductCross.Create(p1, p3));
        await Context.SaveChangesAsync();

        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = p1,
                CrossProductId = p2,
                LinkageType = ProductLinkageType.FullCross
            }
        ]);

        await TestContext.Mediator.Send(command);

        var crosses = await Context
            .ProductCrosses
            .AsNoTracking()
            .ToListAsync();

        crosses.Should().Contain(x => x.LeftProductId == Math.Min(p1, p2)
                                      && x.RightProductId == Math.Max(p1, p2));

        crosses.Should().Contain(x => x.LeftProductId == Math.Min(p3, p2)
                                      && x.RightProductId == Math.Max(p3, p2));
    }
    
    [Fact]
    public async Task MakeLinkage_FullLeftToRightCross_CreatesCorrectLinks()
    {
        var p1 = TestContext.Products[0].Id;
        var p2 = TestContext.Products[1].Id;
        var p3 = TestContext.Products[2].Id;

        await Context.ProductCrosses.AddAsync(ProductCross.Create(p1, p3));
        await Context.SaveChangesAsync();

        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = p1,
                CrossProductId = p2,
                LinkageType = ProductLinkageType.FullLeftToRightCross
            }
        ]);

        await TestContext.Mediator.Send(command);

        var crosses = await Context.ProductCrosses.AsNoTracking().ToListAsync();

        crosses.Should().Contain(x => x.RightProductId == p2 && x.LeftProductId == Math.Min(p1, p2));
        crosses.Should().Contain(x => x.RightProductId == p2 && x.LeftProductId == Math.Min(p3, p2));
    }
    
    [Fact]
    public async Task MakeLinkage_FullRightToLeftCross_CreatesCorrectLinks()
    {
        var p1 = TestContext.Products[0].Id;
        var p2 = TestContext.Products[1].Id;
        var p3 = TestContext.Products[2].Id;

        await Context.ProductCrosses.AddAsync(ProductCross.Create(p2, p3));
        await Context.SaveChangesAsync();

        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = p1,
                CrossProductId = p2,
                LinkageType = ProductLinkageType.FullRightToLeftCross
            }
        ]);

        await TestContext.Mediator.Send(command);

        var crosses = await Context.ProductCrosses.AsNoTracking().ToListAsync();

        crosses.Should().Contain(x => x.LeftProductId == Math.Min(p1, p2)
                                      && x.RightProductId == Math.Max(p1, p2));

        crosses.Should().Contain(x => x.LeftProductId == Math.Min(p1, p3)
                                      && x.RightProductId == Math.Max(p1, p3));
    }
    
    [Fact]
    public async Task MakeLinkage_ProductNotFound_Throws()
    {
        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = 999999,
                CrossProductId = TestContext.Products[0].Id,
                LinkageType = ProductLinkageType.SingleCross
            }
        ]);

        var act = () => TestContext.Mediator.Send(command);

        await act.Should().ThrowAsync<DbValidationException>();
    }
    
    [Fact]
    public async Task MakeLinkage_FullCross_DoesNotCreateDuplicates()
    {
        var p1 = TestContext.Products[0].Id;
        var p2 = TestContext.Products[1].Id;

        await Context.ProductCrosses.AddAsync(ProductCross.Create(p1, p2));
        await Context.SaveChangesAsync();

        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = p1,
                CrossProductId = p2,
                LinkageType = ProductLinkageType.FullCross
            }
        ]);

        await TestContext.Mediator.Send(command);

        var crosses = await Context.ProductCrosses
            .Where(x => x.LeftProductId == Math.Min(p1, p2)
                        && x.RightProductId == Math.Max(p1, p2))
            .ToListAsync();

        crosses.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task MakeLinkage_MultipleLinkages_Succeeds()
    {
        var p1 = TestContext.Products[0].Id;
        var p2 = TestContext.Products[1].Id;
        var p3 = TestContext.Products[2].Id;

        var command = new MakeLinkageBetweenProductsCommand([
            new NewProductLinkageDto
            {
                ProductId = p1,
                CrossProductId = p2,
                LinkageType = ProductLinkageType.SingleCross
            },
            new NewProductLinkageDto
            {
                ProductId = p2,
                CrossProductId = p3,
                LinkageType = ProductLinkageType.SingleCross
            }
        ]);

        await TestContext.Mediator.Send(command);

        var crosses = await Context.ProductCrosses.AsNoTracking().ToListAsync();

        crosses.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}