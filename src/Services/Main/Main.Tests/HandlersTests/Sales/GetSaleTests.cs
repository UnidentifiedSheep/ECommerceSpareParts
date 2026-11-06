using FluentValidation.TestHelper;
using Tests.Abstractions.Test;
using FluentAssertions;
using Main.Application.Handlers.Sales.GetSale;
using Main.Entities.Exceptions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public class GetSaleTests : IntegrationTest
{
	public GetSaleTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext SaleContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task GetSale_BySaleId_ReturnsSale()
	{
		var sale = SaleContext.Sale;

		var result = await Mediator.Send(new GetSaleQuery(sale.Id, null), CancellationToken);

		result.Sale.Id.Should().Be(sale.Id);
		result.Sale.TransactionId.Should().Be(sale.TransactionId);
		result.Sale.Buyer.Id.Should().Be(SaleContext.Buyer.Id);
		result.Sale.Currency.Id.Should().Be(sale.CurrencyId);
		result.Sale.StorageCode.Should().Be(sale.StorageCode);
		result.Sale.State.Should().Be(sale.State);
		result.Sale.RowVersion.Should().Be(sale.RowVersion);
	}

	[Fact]
	public async Task GetSale_ByTransactionId_ReturnsSale()
	{
		var sale = SaleContext.Sale;

		var result = await Mediator.Send(new GetSaleQuery(null, sale.TransactionId), CancellationToken);

		result.Sale.Id.Should().Be(sale.Id);
		result.Sale.TransactionId.Should().Be(sale.TransactionId);
	}

	[Fact]
	public async Task GetSale_WhenSaleDoesNotExist_ThrowsSaleNotFoundException()
	{
		await Assert.ThrowsAsync<SaleNotFoundException>(() => Mediator.Send(
			new GetSaleQuery(Guid.NewGuid(), null),
			CancellationToken));
	}

	[Fact]
	public async Task GetSale_WhenTransactionDoesNotExist_ThrowsSaleNotFoundException()
	{
		await Assert.ThrowsAsync<SaleNotFoundException>(() => Mediator.Send(
			new GetSaleQuery(null, Guid.NewGuid()),
			CancellationToken));
	}


}


public class GetSaleValidationTests : ValidationTest<GetSaleValidation, GetSaleQuery>
{
	[Fact]
	public async Task MissingSaleAndTransactionIds_FailsValidation()
	{
		var result = await ValidateAsync(new GetSaleQuery(null, null));
		result.ShouldHaveValidationErrorFor(x => x)
			.WithErrorCode(Main.Entities.SaleIdOrTransactionIdRequiredMessage.Instance.MessageKey).Only();
	}
}
