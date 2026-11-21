using Application.Common.Models;
using Application.Common.Services;
using Enums;
using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Commands.StorageContents.EditContent;
using Main.Modules.Storage.Application.Dtos.Storage;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class EditStorageContentTests : IntegrationTest
{
	public EditStorageContentTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageContentTestContext>();

	public StorageContentTestContext TestContext => GetContext<StorageContentTestContext>();

	[Fact]
	public async Task EditStorageContent_WithInvalidStorageContentId_ThrowsStorageContentNotFoundException()
	{
		var dto = new PatchStorageContentDto
		{
			Count = new PatchField<int>
			{
				IsSet = true, Value = 6
			}
		};
		var content = TestContext.StorageContents.First();
		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[999999] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);

		await Assert.ThrowsAsync<StorageContentNotFoundException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task EditStorageContent_ValidInput_Succeeds()
	{
		var content = TestContext.StorageContents.First();
		var productBeforeStockQuantity =
			await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);
		var dto = new PatchStorageContentDto
		{
			Count = new PatchField<int>
			{
				IsSet = true, Value = content.Count + 5
			},
			BuyPrice = new PatchField<decimal>
			{
				IsSet = true, Value = content.BuyPrice + 10
			}
		};

		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[content.Id] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);
		await Mediator.Send(command, CancellationToken);

		var updated = await Context.Set<StorageContent>()
			.FindAsync(
				new object?[]
				{
					content.Id
				},
				CancellationToken);
		var productAfterStockQuantity =
			await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);

		Assert.Equal(productBeforeStockQuantity, productAfterStockQuantity - 5);
		Assert.NotNull(updated);
		Assert.Equal(content.Count, dto.Count.Value);
		Assert.Equal(content.BuyPrice, dto.BuyPrice.Value);
		var persisted = await Context.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(item => item.Id == content.Id, CancellationToken);
		Assert.Equal(StorageMovementType.StorageContentEditing, persisted.Metadata.LastMovementType);

	}

	[Fact]
	public async Task EditStorageContent_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
	{
		var content = TestContext.StorageContents.First();
		var dto = new PatchStorageContentDto
		{
			CurrencyId = new PatchField<int>
			{
				IsSet = true, Value = 99999
			}
		};
		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[content.Id] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);
		await Assert.ThrowsAsync<DbValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task EditStorageContent_WithMultipleFieldsUpdate_Succeeds()
	{
		var content = TestContext.StorageContents.First();
		var dto = new PatchStorageContentDto
		{
			Count = new PatchField<int>
			{
				IsSet = true, Value = content.Count + 3
			},
			BuyPrice = new PatchField<decimal>
			{
				IsSet = true, Value = content.BuyPrice + 7
			},
			CurrencyId = new PatchField<int>
			{
				IsSet = true, Value = 1
			}
		};

		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[content.Id] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);

		await Mediator.Send(command, CancellationToken);

		var updated = await Context.Set<StorageContent>()
			.FindAsync(
				new object?[]
				{
					content.Id
				},
				CancellationToken);
		Assert.NotNull(updated);
		Assert.Equal(content.Count, dto.Count.Value);
		Assert.Equal(content.BuyPrice, dto.BuyPrice.Value);
		var persisted = await Context.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(item => item.Id == content.Id, CancellationToken);
		Assert.Equal(StorageMovementType.StorageContentEditing, persisted.Metadata.LastMovementType);
	}
}

public class
	EditStorageContentValidationTests : ValidationTest<EditStorageContentValidation,
	EditStorageContentCommand>
{
	protected override EditStorageContentValidation CreateValidator()
	{
		var policy = new Mock<IOperationDatePolicy>();
		policy.Setup(x => x.IsAllowed(It.IsAny<DateTime>()))
			.Returns(OperationDateValidationResult.Valid());
		return new EditStorageContentValidation(policy.Object);
	}

	[Fact]
	public async Task EditStorageContent_WithNegativeCount_ThrowsException()
	{
		var dto = new PatchStorageContentDto
		{
			Count = new PatchField<int>
			{
				IsSet = true, Value = -1
			}
		};
		var content = new { Id = 1, RowVersion = 1u };

		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[content.Id] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("EditedFields.Values[0].Model.Count.Value");
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
				IsSet = true, Value = price
			}
		};

		var content = new { Id = 1, RowVersion = 1u };
		var dict = new Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>>
		{
			[content.Id] = new(dto, content.RowVersion)
		};

		var command = new EditStorageContentCommand(dict);

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("EditedFields.Values[0].Model.BuyPrice.Value");
	}
}
