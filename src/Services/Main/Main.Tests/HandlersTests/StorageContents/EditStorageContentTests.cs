using Abstractions.Models;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Storage;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.StorageContents;

public class EditStorageContentTests : IntegrationTest
{
    public EditStorageContentTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
    }

    public StorageContentTestContext TestContext => GetContext<StorageContentTestContext>();

    [Fact]
    public async Task EditStorageContent_WithNegativeCount_ThrowsException()
    {
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = -1
            }
        };
        var content = TestContext.StorageContents.First();

        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
        {
            [content.Id] = new(dto, content.RowVersion)
        };

        var command = new EditStorageContentCommand(dict);

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(-1)]
    public async Task EditStorageContent_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(
        decimal price)
    {
        var dto = new PatchStorageContentDto
        {
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = price
            }
        };

        var content = TestContext.StorageContents.First();
        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
            { [content.Id] = new(dto, content.RowVersion) };

        var command = new EditStorageContentCommand(dict);

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageContent_WithInvalidStorageContentId_ThrowsStorageContentNotFoundException()
    {
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = 6
            }
        };
        var content = TestContext.StorageContents.First();
        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
            { [999999] = new(dto, content.RowVersion) };

        var command = new EditStorageContentCommand(dict);

        await Assert.ThrowsAsync<StorageContentNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageContent_ValidInput_Succeeds()
    {
        var content = TestContext.StorageContents.First();
        var productBefore = await Context.Products
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ProductId);
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = content.Count + 5
            },
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = content.BuyPrice + 10
            }
        };

        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
            { [content.Id] = new(dto, content.RowVersion) };

        var command = new EditStorageContentCommand(dict);
        await Mediator.Send(command);

        var updated = await Context.StorageContents.FindAsync(content.Id);
        var productAfter = await Context.Products
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ProductId);

        Assert.Equal(productBefore.Stock.Value, productAfter.Stock.Value - 5);
        Assert.NotNull(updated);
        Assert.Equal(content.Count, dto.Count.Value);
        Assert.Equal(content.BuyPrice, dto.BuyPrice.Value);
    }

    [Fact]
    public async Task EditStorageContent_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        var content = TestContext.StorageContents.First();
        var dto = new PatchStorageContentDto
        {
            CurrencyId = new PatchField<int>
            {
                IsSet = true,
                Value = 99999
            }
        };
        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
            { [content.Id] = new(dto, content.RowVersion) };

        var command = new EditStorageContentCommand(dict);
        await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
    }


    [Fact]
    public async Task EditStorageContent_WithMultipleFieldsUpdate_Succeeds()
    {
        var content = TestContext.StorageContents.First();
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = content.Count + 3
            },
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = content.BuyPrice + 7
            },
            CurrencyId = new PatchField<int>
            {
                IsSet = true,
                Value = 1
            }
        };

        var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
            { [content.Id] = new(dto, content.RowVersion) };

        var command = new EditStorageContentCommand(dict);

        await Mediator.Send(command);

        var updated = await Context.StorageContents.FindAsync(content.Id);
        Assert.NotNull(updated);
        Assert.Equal(content.Count, dto.Count.Value);
        Assert.Equal(content.BuyPrice, dto.BuyPrice.Value);
    }
}