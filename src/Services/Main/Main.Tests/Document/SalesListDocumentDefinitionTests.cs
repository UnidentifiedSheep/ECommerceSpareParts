using System.Globalization;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Enums;
using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Document.Interfaces.Document;
using Main.Entities.Sale;
using Main.Entities.Sale.Enums;
using Main.Entities.Sale.Exceptions;
using Main.Modules.Currency.Abstractions.Models;
using Main.Modules.Identity.Abstractions.Models;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Organization.Abstractions.Models;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Abstractions.Models;
using Main.Modules.Sale.Application.Document.Sales.SalesList;
using Main.Modules.Sale.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Options;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.Document;

public sealed class SalesListDocumentDefinitionTests : IntegrationTest
{
	public SalesListDocumentDefinitionTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<SaleTestContext>();

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task NoFilters_ReturnsSalesItemsAndTotals()
	{
		var sale = TestContext.Sale;
		var content = sale.Contents.Single();
		var currency = await Scope.ServiceProvider.GetRequiredService<IDisplay<int, CurrencyDisplayInfo>>()
			.GetAsync(sale.CurrencyId, CancellationToken);

		var schema = await GetSchemaAsync(Request());

		schema.OrganizationsCount.Should().Be(1);
		schema.SalesCount.Should().Be(1);
		schema.ItemsCount.Should().Be(1);
		schema.Period.Should().Be("All time");

		var organization = schema.Organizations.Should().ContainSingle().Subject;
		organization.OrganizationName.Should().NotBe("—");
		organization.SalesCount.Should().Be(1);
		organization.TotalsText.Should()
			.Be($"{content.TotalSum.ToString("N2", CultureInfo.GetCultureInfo("en"))} {currency!.Sign}");

		var saleRow = organization.Sales.Should().ContainSingle().Subject;
		saleRow.SaleDatetime.Should().BeCloseTo(sale.SaleDatetime.ToUniversalTime(), TimeSpan.FromMilliseconds(1));
		saleRow.BuyerName.Should().NotBe("—");
		saleRow.StorageCode.Should().Be(sale.StorageCode);
		saleRow.CurrencyCode.Should().Be(currency.Code);
		saleRow.TotalSum.Should().Be(content.TotalSum);

		var item = saleRow.Items.Should().ContainSingle().Subject;
		item.Sku.Should().Be(TestContext.Product.Sku.Value);
		item.ProductName.Should().Be(TestContext.Product.Name);
		item.Count.Should().Be(content.Count);
		item.TotalSum.Should().Be(content.TotalSum);

		var total = schema.Totals.Should().ContainSingle().Subject;
		total.CurrencyCode.Should().Be(currency.Code);
		total.SalesCount.Should().Be(1);
		total.ItemsCount.Should().Be(1);
		total.TotalSum.Should().Be(content.TotalSum);
	}

	[Fact]
	public async Task DateRange_IsFormattedAsPeriod()
	{
		var schema = await GetSchemaAsync(Request() with
		{
			Culture = "ru",
			RangeStartDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			RangeEndDate = new DateTime(2100, 1, 31, 0, 0, 0, DateTimeKind.Utc)
		});

		schema.Period.Should().Be("01.01.2000 – 31.01.2100");
		schema.Organizations.Should().ContainSingle();
	}

	[Fact]
	public async Task Filters_AreApplied()
	{
		var schema = await GetSchemaAsync(Request() with { States = [SaleState.Draft] });

		schema.Organizations.Should().BeEmpty();
		schema.Totals.Should().BeEmpty();
		schema.SalesCount.Should().Be(0);
	}

	[Fact]
	public async Task StartAfterEnd_Throws()
	{
		var act = () => GetSchemaAsync(Request() with
		{
			RangeStartDate = new DateTime(2030, 1, 2, 0, 0, 0, DateTimeKind.Utc),
			RangeEndDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		});

		await act.Should().ThrowAsync<SalesDocumentInvalidDateRangeException>();
	}

	private static SalesListDocumentRequest Request()
		=> new()
		{
			DocumentType = DocumentType.Excel,
			Culture = "en"
		};

	private Task<SalesListDocumentSchema> GetSchemaAsync(SalesListDocumentRequest request)
		=> ActivatorUtilities.CreateInstance<TestableDefinition>(Scope.ServiceProvider)
			.GetSchemaAsync(request, CancellationToken);

	private sealed class TestableDefinition(
		IDocumentTemplateResolver templateResolver,
		IS3Service s3Service,
		IReadRepository<Main.Entities.Sale.Sale.Sale, Guid> repository,
		ISalesFilterApplier filterApplier,
		IDisplay<Guid, UserDisplayInfo> userDisplay,
		IDisplay<int, CurrencyDisplayInfo> currencyDisplay,
		IDisplay<Guid, OrganizationDisplayInfo> organizationDisplay,
		IDisplay<int, ProductDisplayInfo> productDisplay,
		ILocalizer localizer,
		TimeProvider timeProvider,
		IOptions<S3BucketsOptions> options) : SalesListDocumentDefinition(
		templateResolver,
		s3Service,
		repository,
		filterApplier,
		userDisplay,
		currencyDisplay,
		organizationDisplay,
		productDisplay,
		localizer,
		timeProvider,
		options)
	{
		public Task<SalesListDocumentSchema> GetSchemaAsync(SalesListDocumentRequest request, CancellationToken token)
			=> GetSchemaDataAsync(request, token);
	}
}
