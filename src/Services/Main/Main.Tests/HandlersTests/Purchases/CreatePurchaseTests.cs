using Application.Common.Services;
using Contracts;
using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Currency;
using Main.Entities.Organization.Exceptions;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using Main.Entities.Purchase.Enums;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Storage;
using Main.Entities.User;
using Main.Modules.Purchase.Application.Dtos.Purchase;
using Main.Modules.Purchase.Application.Handlers.Purchases.CreatePurchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class CreatePurchaseTests : IntegrationTest
{
	private User _supplier = null!;

	public CreatePurchaseTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<UsersTestContext>();
	}

	private MessageBrokerStub Broker
		=> (MessageBrokerStub)Scope.ServiceProvider
			.GetRequiredService<PublishEndpointAccessor>()
			.Publisher;

	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();

		_supplier = await new SupplierUserBuilder(Faker).BuildAndAddToDb(Context);
	}

	[Fact]
	public async Task CreatePurchase_ForBusinessOrganization_UsesOrganizationForTransaction()
	{
		var organization = await new OrganizationBuilder(Faker)
			.WithOwnerId(_supplier.Id)
			.WithName("Supplier organization")
			.WithSystemName($"supplier-{Guid.NewGuid():N}")
			.BuildAndAddToDb(Context, CancellationToken);

		var command = CreateValidCommand() with
		{
			SupplierOrganizationId = organization.Id
		};

		var result = await Mediator.Send(command, CancellationToken);

		var purchase = await Context
			.Set<Purchase>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == result.PurchaseId, CancellationToken);
		purchase.SupplierUserId.Should().Be(_supplier.Id);
		purchase.SupplierOrganizationId.Should().Be(organization.Id);

		var transaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == purchase.TransactionId, CancellationToken);
		transaction.SenderId.Should().Be(organization.Id);
	}

	[Fact]
	public async Task CreatePurchase_WhenOrganizationMemberHasNoSupplierRole_CreatesPurchase()
	{
		var member = GetContext<UsersTestContext>().Users.First();
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var product = GetContext<ProductTestContext>().Products.First();
		var command = CreateCommand(
			member.Id,
			currency.Id,
			storage.Code,
			[
				NewContent(product, 1)
			]);

		var result = await Mediator.Send(command, CancellationToken);
		var purchase = await Context
			.Set<Purchase>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == result.PurchaseId, CancellationToken);

		purchase.SupplierUserId.Should().Be(member.Id);
		purchase.SupplierOrganizationId.Should().Be(member.Id);
	}

	[Fact]
	public async Task CreatePurchase_CreatesPurchaseAndStorageContents()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var products = GetContext<ProductTestContext>().Products.Take(2).ToList();
		var command = CreateCommand(
			_supplier.Id,
			currency.Id,
			storage.Code,
			products.Select((product, index) => NewContent(
				product,
				index + 1)));

		var publishedBefore = Broker.PublishedMessages.Count;
		await Mediator.Send(command, CancellationToken);

		var purchase = await Context
			.Set<Purchase>()
			.Include(x => x.Contents)
			.AsNoTracking()
			.SingleAsync(CancellationToken);

		purchase.SupplierUserId.Should().Be(_supplier.Id);
		purchase.SupplierOrganizationId.Should().Be(_supplier.Id);
		purchase.CurrencyId.Should().Be(currency.Id);
		purchase.Storage.Should().Be(storage.Code);
		purchase.State.Should().Be(PurchaseState.Completed);
		purchase.Comment.Should().Be(command.Comment);
		purchase.Contents.Should().HaveCount(command.PurchaseContent.Count());

		var storageContents =
			await Context.Set<StorageContent>().AsNoTracking().ToListAsync(CancellationToken);
		storageContents.Should().HaveCount(command.PurchaseContent.Count());
		foreach (var item in command.PurchaseContent)
			storageContents
				.Should()
				.Contain(x =>
					x.ProductId == item.ProductId && x.Count == item.Count && x.BuyPrice == item.Price &&
					x.CurrencyId == command.CurrencyId && x.StorageCode == command.StorageCode);

		var purchaseTransaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == purchase.TransactionId, CancellationToken);
		purchaseTransaction.SenderId.Should().Be(_supplier.Id);
		purchaseTransaction.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
		purchaseTransaction.Amount.Should().Be(command.PurchaseContent.Sum(x => x.Price * x.Count));
		purchaseTransaction.SourceType.Should().Be(TransactionSourceType.Purchase);

		var published = Broker.PublishedMessages.Skip(publishedBefore).ToArray();
		published.OfType<PurchaseCreatedEvent>().Should().ContainSingle().Which.Id.Should().Be(purchase.Id);
		published.OfType<PurchaseUpdatedEvent>().Should().BeEmpty();
		published.OfType<PurchaseDeletedEvent>().Should().BeEmpty();
	}

	[Fact]
	public async Task CreatePurchase_WithPayedSum_CreatesPaymentTransaction()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var product = GetContext<ProductTestContext>().Products.First();
		var payedSum = 5m;
		var command = CreateCommand(
			_supplier.Id,
			currency.Id,
			storage.Code,
			[
				NewContent(product, 1)
			],
			payedSum);

		await Mediator.Send(command, CancellationToken);

		var transactions = await Context.Set<Transaction>().AsNoTracking().ToListAsync(CancellationToken);
		transactions.Should().HaveCount(2);

		var paymentTransaction = transactions.Single(x => x.Amount == payedSum);
		paymentTransaction.SenderId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
		paymentTransaction.ReceiverId.Should().Be(_supplier.Id);
		paymentTransaction.CurrencyId.Should().Be(currency.Id);
		paymentTransaction.SourceType.Should().Be(TransactionSourceType.Manual);
	}

	[Fact]
	public async Task CreatePurchase_WithOverpaymentAndForcePayment_CreatesPurchase()
	{
		var command = CreateValidCommand() with
		{
			PayedSum = 9_999m, ForcePayment = true
		};

		await Mediator.Send(command, CancellationToken);

		var supplierBalance = await Context
			.Set<OrganizationBalance>()
			.AsNoTracking()
			.SingleAsync(
				x => x.OrganizationId == command.SupplierOrganizationId && x.CurrencyId == command.CurrencyId,
				CancellationToken);
		var purchaseTotal = command.PurchaseContent.Sum(x => x.Price * x.Count);

		supplierBalance.Balance.Should().Be(purchaseTotal - command.PayedSum.Value);
	}

	[Fact]
	public async Task CreatePurchase_WithZeroPayedSum_DoesNotCreatePaymentTransaction()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var product = GetContext<ProductTestContext>().Products.First();
		var command = CreateCommand(
			_supplier.Id,
			currency.Id,
			storage.Code,
			[
				NewContent(product, 1)
			],
			0m);

		await Mediator.Send(command, CancellationToken);

		var transactions = await Context.Set<Transaction>().AsNoTracking().ToListAsync(CancellationToken);
		transactions.Should().HaveCount(1);
	}

	[Fact]
	public async Task CreatePurchase_WithMissingSupplierOrganization_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			SupplierOrganizationId = Guid.NewGuid()
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is OrganizationNotFoundException);
	}

	[Fact]
	public async Task CreatePurchase_WhenSupplierIsNotOrganizationMember_ThrowsDbValidationException()
	{
		var otherOrganizationId = GetContext<UsersTestContext>().Users.First().Id;
		var command = CreateValidCommand() with
		{
			SupplierOrganizationId = otherOrganizationId
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is OrganizationMemberNotFoundException);
	}

	[Fact]
	public async Task CreatePurchase_WithMissingCurrency_ThrowsDbValidationException()
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
	public async Task CreatePurchase_WithMissingStorage_ThrowsDbValidationException()
	{
		var command = CreateValidCommand() with
		{
			StorageCode = Faker.Lorem.Letter(200)
		};

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		exception.Failures[0].Error.Should().BeOfType<StorageNotFoundException>();
	}

	[Fact]
	public async Task CreatePurchase_WithMissingProduct_ThrowsProductNotFoundException()
	{
		var command = CreateValidCommand();
		var content = command.PurchaseContent.Single() with
		{
			ProductId = int.MaxValue
		};

		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(
			command with
			{
				PurchaseContent = [content]
			},
			CancellationToken));
		Assert.Contains(exception.Failures, failure => failure.Error is ProductNotFoundException);
	}

	private CreatePurchaseCommand CreateCommand(
		Guid supplierUserId,
		int currencyId,
		string storageCode,
		IEnumerable<NewPurchaseContentDto> contents,
		decimal? payedSum = null,
		Guid? supplierOrganizationId = null)
		=> new(
			supplierUserId,
			supplierOrganizationId ?? supplierUserId,
			currencyId,
			storageCode,
			DateTime.UtcNow,
			contents.ToList(),
			Faker.Lorem.Sentence(),
			payedSum);

	private CreatePurchaseCommand CreateValidCommand()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var product = GetContext<ProductTestContext>().Products.First();

		return CreateCommand(
			_supplier.Id,
			currency.Id,
			storage.Code,
			[
				NewContent(product, 1)
			]);
	}

	private static NewPurchaseContentDto NewContent(
		Product product,
		int count)
		=> new()
		{
			ProductId = product.Id,
			Count = count,
			Price = 10m + count,
			Comment = $"content-{count}"
		};
}

public class CreatePurchaseValidationTests : ValidationTest<CreatePurchaseValidation, CreatePurchaseCommand>
{
	protected override CreatePurchaseValidation CreateValidator()
	{
		var policy = new Mock<IOperationDatePolicy>();
		policy.Setup(x => x.IsAllowed(It.IsAny<DateTime>())).Returns(OperationDateValidationResult.Valid());
		return new CreatePurchaseValidation(policy.Object);
	}

	private static CreatePurchaseCommand CreateValidCommand()
		=> new(
			Guid.NewGuid(),
			Guid.NewGuid(),
			1,
			"storage",
			DateTime.UtcNow,
			[
				new NewPurchaseContentDto
					{ ProductId = 1, Count = 1, Price = 100m }
			],
			null,
			null);

	[Fact]
	public async Task CreatePurchase_WithEmptyContent_FailsValidation()
	{
		var command = CreateValidCommand() with
		{
			PurchaseContent = []
		};

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.PurchaseContent);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task CreatePurchase_WithInvalidContentCount_FailsValidation(int count)
	{
		var command = CreateValidCommand();
		var content = command.PurchaseContent.Single() with
		{
			Count = count
		};

		var result = await ValidateAsync(
			command with
			{
				PurchaseContent = [content]
			});
		result.ShouldHaveValidationErrorFor("PurchaseContent[0].Count");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(0.001)]
	public async Task CreatePurchase_WithInvalidContentPrice_FailsValidation(decimal price)
	{
		var command = CreateValidCommand();
		var content = command.PurchaseContent.Single() with
		{
			Price = price
		};

		var result = await ValidateAsync(
			command with
			{
				PurchaseContent = [content]
			});
		result.ShouldHaveValidationErrorFor("PurchaseContent[0].Price");
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0.001)]
	public async Task CreatePurchase_WithInvalidPayedSum_FailsValidation(decimal payedSum)
	{
		var command = CreateValidCommand() with
		{
			PayedSum = payedSum
		};

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.PayedSum);
	}

	[Fact]
	public async Task CreatePurchase_WithEmptySupplierUserId_FailsValidation()
	{
		var command = CreateValidCommand() with
		{
			SupplierUserId = Guid.Empty
		};

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.SupplierUserId);
	}

	[Fact]
	public async Task CreatePurchase_WithEmptySupplierOrganizationId_FailsValidation()
	{
		var command = CreateValidCommand() with
		{
			SupplierOrganizationId = Guid.Empty
		};

		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.SupplierOrganizationId);
	}
}
