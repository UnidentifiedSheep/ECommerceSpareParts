using Exceptions;
using FluentAssertions;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Sales;

public class CreateSaleTests : IntegrationTest
{
	public CreateSaleTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<StorageContentTestContext>();
		RegisterBasicContext<UsersTestContext>();
	}

	[Fact]
	public async Task CreateSale_CreatesSaleAndSubtractsStorageContent()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var productStockQuantity = await Context.GetProductStockQuantityAsync(storageContent.ProductId, CancellationToken);
		var originalContents = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x =>
				x.ProductId == storageContent.ProductId && x.StorageCode == storageContent.StorageCode)
			.ToDictionaryAsync(x => x.Id, CancellationToken);
		var originalStock = productStockQuantity;
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[
				NewContent(
					storageContent.ProductId,
					1,
					100m,
					80m)
			]);

		var result = await Mediator.Send(command, CancellationToken);

		result.SaleId.Should().NotBeEmpty();

		var sale = await Context
			.Sales
			.Include(x => x.Contents)
			.ThenInclude(x => x.Details)
			.AsNoTracking()
			.SingleAsync(x => x.Id == result.SaleId, CancellationToken);
		sale.UserId.Should().Be(buyer.Id);
		sale.OrganizationId.Should().Be(buyer.Id);
		sale.CurrencyId.Should().Be(storageContent.CurrencyId);
		sale.StorageCode.Should().Be(storageContent.StorageCode);
		sale.Comment.Should().Be(command.Comment);

		var saleContent = sale.Contents.Should().ContainSingle().Subject;
		saleContent.ProductId.Should().Be(storageContent.ProductId);
		saleContent.Count.Should().Be(1);
		saleContent.DiscountedPrice.Should().Be(80m);
		saleContent.TotalSum.Should().Be(80m);
		saleContent.OriginalPrice.Should().Be(100);
		var analytics = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(item => item.SaleContentId == saleContent.Id, CancellationToken);
		analytics.OriginalAmountInBaseCurrency.Should().Be(100m * saleContent.Count);
		analytics.RevenueInBaseCurrency.Should().Be(80m * saleContent.Count);

		var detail = saleContent.Details.Should().ContainSingle().Subject;
		originalContents.Should().ContainKey(detail.StorageContentId);
		var affectedStorageContent = originalContents[detail.StorageContentId];
		detail.CurrencyId.Should().Be(affectedStorageContent.CurrencyId);
		detail.BuyPrice.Should().Be(affectedStorageContent.BuyPrice);
		detail.Count.Should().Be(1);
		detail
			.PurchaseDatetime
			.Should()
			.BeCloseTo(
				affectedStorageContent.PurchaseDatetime.ToUniversalTime(),
				TimeSpan.FromMilliseconds(1));

		var updatedContent = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == detail.StorageContentId, CancellationToken);
		updatedContent.Count.Should().Be(affectedStorageContent.Count - 1);

		var updatedProductStockQuantity = await Context.GetProductStockQuantityAsync(storageContent.ProductId, CancellationToken);
		updatedProductStockQuantity.Should().Be(originalStock - 1);

		var saleTransaction = await Context
			.Transactions
			.AsNoTracking()
			.SingleAsync(x => x.Id == sale.TransactionId, CancellationToken);
		saleTransaction.SenderId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
		saleTransaction.ReceiverId.Should().Be(buyer.Id);
		saleTransaction.Amount.Should().Be(80m);
		saleTransaction.CurrencyId.Should().Be(storageContent.CurrencyId);
		saleTransaction.SourceType.Should().Be(TransactionSourceType.Sale);
	}

	[Fact]
	public async Task CreateSale_WithPayedSum_CreatesPaymentTransaction()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var payedSum = 50m;
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[NewContent(storageContent.ProductId, 1)],
			payedSum);

		await Mediator.Send(command, CancellationToken);

		var transactions = await Context.Transactions.AsNoTracking().ToListAsync(CancellationToken);
		transactions.Should().HaveCount(2);

		var payment = transactions.Single(x => x.SourceType == TransactionSourceType.Manual);
		payment.SenderId.Should().Be(buyer.Id);
		payment.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
		payment.Amount.Should().Be(payedSum);
		payment.CurrencyId.Should().Be(storageContent.CurrencyId);
	}

	[Fact]
	public async Task CreateSale_WithFullPayedSumAndZeroBalanceWithoutForcePayment_CreatesSale()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[
				NewContent(
					storageContent.ProductId,
					1,
					1200m,
					1200m)
			],
			1200m,
			forcePayment: false);

		await Mediator.Send(command, CancellationToken);

		var balance = await Context
			.UserBalances
			.AsNoTracking()
			.SingleAsync(
				x => x.OrganizationId == buyer.Id && x.CurrencyId == storageContent.CurrencyId,
				CancellationToken);
		balance.Balance.Should().Be(0m);
	}

	[Fact]
	public async Task CreateSale_WithoutPayedSumAndZeroBalanceWithoutForcePayment_Throws()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[
				NewContent(
					storageContent.ProductId,
					1,
					1200m,
					1200m)
			],
			forcePayment: false);

		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task CreateSale_WithFullPayedSumAndZeroBalanceWithForcePayment_CreatesSale()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[
				NewContent(
					storageContent.ProductId,
					1,
					1200m,
					1200m)
			],
			1200m);

		await Mediator.Send(command, CancellationToken);

		var balance = await Context
			.UserBalances
			.AsNoTracking()
			.SingleAsync(
				x => x.OrganizationId == buyer.Id && x.CurrencyId == storageContent.CurrencyId,
				CancellationToken);
		balance.Balance.Should().Be(0m);
	}

	[Fact]
	public async Task CreateSale_WithZeroPayedSum_DoesNotCreatePaymentTransaction()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[NewContent(storageContent.ProductId, 1)],
			0m);

		await Mediator.Send(command, CancellationToken);

		var transactions = await Context.Transactions.AsNoTracking().ToListAsync(CancellationToken);
		transactions.Should().ContainSingle();
		transactions.Single().SourceType.Should().Be(TransactionSourceType.Sale);
	}

	[Fact]
	public async Task CreateSale_WithDuplicateProductContents_AggregatesStockCheckAndCreatesSeparateContents()
	{
		var buyer = Buyer();
		var storageContent = StorageContentWithCountAtLeast(2);
		var countOnStorage = await CountOnStorage(storageContent);
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[
				NewContent(storageContent.ProductId, 1),
				NewContent(
					storageContent.ProductId,
					1,
					200m,
					180m)
			]);

		var result = await Mediator.Send(command, CancellationToken);

		var sale = await Context
			.Sales
			.Include(x => x.Contents)
			.ThenInclude(x => x.Details)
			.AsNoTracking()
			.SingleAsync(x => x.Id == result.SaleId, CancellationToken);
		sale.Contents.Sum(x => x.TotalSum).Should().Be(270m);
		sale.Contents.Should().HaveCount(2);
		sale.Contents.Sum(x => x.Count).Should().Be(2);
		sale.Contents.SelectMany(x => x.Details).Sum(x => x.Count).Should().Be(2);

		var updatedCountOnStorage = await CountOnStorage(storageContent);
		updatedCountOnStorage.Should().Be(countOnStorage - 2);
	}

	[Fact]
	public async Task CreateSale_WhenStorageCountIsNotEnough_ThrowsNotEnoughCountOnStorageException()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();
		var countOnStorage = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x =>
				x.ProductId == storageContent.ProductId && x.StorageCode == storageContent.StorageCode)
			.SumAsync(x => x.Count, CancellationToken);
		var command = CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[NewContent(storageContent.ProductId, countOnStorage + 1)]);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Mediator.Send(
			command,
			CancellationToken));

		var salesCount = await Context.Sales.AsNoTracking().CountAsync(CancellationToken);
		salesCount.Should().Be(0);
	}

	[Fact]
	public async Task CreateSale_WithEmptyContents_ThrowsValidationException()
	{
		var storageContent = StorageContent();
		var command = CreateValidCommand() with
		{
			CurrencyId = storageContent.CurrencyId,
			StorageCode = storageContent.StorageCode,
			Contents = []
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command, CancellationToken));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task CreateSale_WithInvalidContentCount_ThrowsValidationException(int count)
	{
		var command = CreateValidCommand();
		var content = command.Contents.Single() with
		{
			Count = count
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(
			command with
			{
				Contents = [content]
			},
			CancellationToken));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(0.001)]
	public async Task CreateSale_WithInvalidContentPrice_ThrowsValidationException(decimal price)
	{
		var command = CreateValidCommand();
		var content = command.Contents.Single() with
		{
			OriginalPrice = price
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(
			command with
			{
				Contents = [content]
			},
			CancellationToken));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(0.001)]
	public async Task CreateSale_WithInvalidPriceWithDiscount_ThrowsValidationException(
		decimal priceWithDiscount)
	{
		var command = CreateValidCommand();
		var content = command.Contents.Single() with
		{
			DiscountedPrice = priceWithDiscount
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(
			command with
			{
				Contents = [content]
			},
			CancellationToken));
	}

	[Fact]
	public async Task CreateSale_WithPriceWithDiscountGreaterThanPrice_ThrowsValidationException()
	{
		var command = CreateValidCommand();
		var content = command.Contents.Single() with
		{
			OriginalPrice = 100m, DiscountedPrice = 101m
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(
			command with
			{
				Contents = [content]
			},
			CancellationToken));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0.001)]
	public async Task CreateSale_WithInvalidPayedSum_ThrowsValidationException(decimal payedSum)
	{
		var command = CreateValidCommand() with
		{
			PayedSum = payedSum
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task CreateSale_WithEmptyUserId_ThrowsValidationException()
	{
		var command = CreateValidCommand() with
		{
			UserId = Guid.Empty
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task CreateSale_WithEmptyOrganizationId_ThrowsValidationException()
	{
		var command = CreateValidCommand() with
		{
			OrganizationId = Guid.Empty
		};

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task CreateSale_WithMissingCurrency_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			CurrencyId = int.MaxValue
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures[0].Error.Should().BeOfType<CurrencyNotFoundException>();
	}

	[Fact]
	public async Task CreateSale_WithMissingStorage_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			StorageCode = "missing-storage"
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures[0].Error.Should().BeOfType<StorageNotFoundException>();
	}

	[Fact]
	public async Task CreateSale_WithMissingUser_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			UserId = Guid.NewGuid()
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures[0].Error.Should().BeOfType<UserNotFoundException>();
	}

	[Fact]
	public async Task CreateSale_WithMissingOrganization_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			OrganizationId = Guid.NewGuid()
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is OrganizationNotFoundException);
	}

	[Fact]
	public async Task CreateSale_WhenUserIsNotOrganizationMember_ThrowsDbValidationException()
	{
		var users = GetContext<UsersTestContext>().Users.ToArray();
		var command = CreateValidCommand() with
		{
			UserId = users[0].Id, OrganizationId = users[1].Id
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is OrganizationMemberNotFoundException);
	}

	[Fact]
	public async Task CreateSale_WithMissingProduct_ThrowsDbValidationException()
	{
		var command = CreateValidCommand();
		var content = command.Contents.Single() with
		{
			ProductId = int.MaxValue
		};

		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(
			command with
			{
				Contents = [content]
			},
			CancellationToken));

		exception.Failures[0].Error.Should().BeOfType<ProductNotFoundException>();
	}

	private CreateSaleCommand CreateValidCommand()
	{
		var buyer = Buyer();
		var storageContent = StorageContent();

		return CreateCommand(
			buyer.Id,
			storageContent.CurrencyId,
			storageContent.StorageCode,
			[NewContent(storageContent.ProductId, 1)]);
	}

	private CreateSaleCommand CreateCommand(
		Guid userId,
		int currencyId,
		string storageCode,
		IEnumerable<NewSaleContentDto> contents,
		decimal? payedSum = null,
		bool forcePayment = true)
	{
		return new CreateSaleCommand(
			userId,
			userId,
			currencyId,
			storageCode,
			DateTime.UtcNow,
			contents.ToList(),
			Faker.Lorem.Sentence(),
			payedSum,
			forcePayment);
	}

	private static NewSaleContentDto NewContent(
		int productId,
		int count,
		decimal originalPrice = 100m,
		decimal discountedPrice = 90m)
	{
		return new NewSaleContentDto
		{
			ProductId = productId,
			Count = count,
			OriginalPrice = originalPrice,
			DiscountedPrice = discountedPrice
		};
	}

	private User Buyer() => GetContext<UsersTestContext>().Users.First();

	private StorageContent StorageContent() =>
		GetContext<StorageContentTestContext>().StorageContents.First(x => x.Count > 0);

	private StorageContent StorageContentWithCountAtLeast(int count)
	{
		return GetContext<StorageContentTestContext>()
			.StorageContents
			.Where(x => x.Count > 0)
			.GroupBy(x => new
			{
				x.ProductId, x.StorageCode
			})
			.OrderByDescending(x => x.Sum(z => z.Count))
			.First(x => x.Sum(z => z.Count) >= count)
			.OrderByDescending(x => x.Count)
			.First();
	}

	private Task<int> CountOnStorage(StorageContent storageContent)
	{
		return Context
			.StorageContents
			.AsNoTracking()
			.Where(x =>
				x.ProductId == storageContent.ProductId && x.StorageCode == storageContent.StorageCode)
			.SumAsync(x => x.Count);
	}
}
