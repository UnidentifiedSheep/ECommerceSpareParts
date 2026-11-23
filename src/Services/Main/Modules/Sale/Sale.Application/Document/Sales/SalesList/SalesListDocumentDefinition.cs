using System.Globalization;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Enums;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Document.Document;
using Main.Application.Document.Interfaces.Document;
using Main.Entities.Sale;
using Main.Entities.Sale.Exceptions;
using Main.Modules.Currency.Abstractions.Models;
using Main.Modules.Identity.Abstractions.Models;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Organization.Abstractions.Models;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Abstractions.Models;
using Main.Modules.Sale.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Options;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public class SalesListDocumentDefinition(
	IDocumentTemplateResolver templateResolver,
	IS3Service s3Service,
	IReadRepository<Entities.Sale.Sale.Sale, Guid> repository,
	ISalesFilterApplier filterApplier,
	IDisplay<Guid, UserDisplayInfo> userDisplay,
	IDisplay<int, CurrencyDisplayInfo> currencyDisplay,
	IDisplay<Guid, OrganizationDisplayInfo> organizationDisplay,
	IDisplay<int, ProductDisplayInfo> productDisplay,
	ILocalizer localizer,
	TimeProvider timeProvider,
	IOptions<S3BucketsOptions> options
) : DocumentDefinitionBase<
	SalesListDocumentRequest,
	SalesListDocumentSchema>(
	templateResolver,
	s3Service,
	options)
{
	public const int MaxItems = 10_000;

	private const string Missing = "—";

	private static readonly DocumentTypePair[] SupportedTypes =
	[
		new(DocumentType.Excel, DocumentType.Excel)
	];

	public override string SystemName => "SalesList";
	public override string DocumentGroup => "Sales";
	public override IReadOnlyCollection<PermissionCodes> RequiredPermissions => [PermissionCodes.SALES_GET];
	public override ILocalizableMessage Name => SaleDocumentListNameMessage.Instance;
	public override ILocalizableMessage Description => SaleDocumentListDescriptionMessage.Instance;
	protected override DocumentTypePair[] SupportedTypePairs => SupportedTypes;

	protected override async Task<SalesListDocumentSchema> GetSchemaDataAsync(
		SalesListDocumentRequest request,
		CancellationToken token)
	{
		if (request.RangeStartDate.HasValue &&
			request.RangeEndDate.HasValue &&
			request.RangeStartDate.Value.Date > request.RangeEndDate.Value.Date)
			throw new SalesDocumentInvalidDateRangeException();

		var culture = DocumentCulture.GetRequired(request.Culture);
		var filtered = filterApplier.Apply(repository.Query, request.ToFilter());

		var itemsCount = await filtered.SelectMany(sale => sale.Contents).CountAsync(token);
		if (itemsCount > MaxItems)
			throw new SalesDocumentTooManyItemsException(itemsCount, MaxItems);

		var sales = await filtered
			.Select(sale => new SaleRow(
				sale.SaleDatetime,
				sale.UserId,
				sale.OrganizationId,
				sale.StorageCode,
				sale.CurrencyId,
				sale.Comment,
				sale.Contents
					.OrderBy(content => content.Id)
					.Select(content => new ContentRow(
						content.ProductId,
						content.Count,
						content.OriginalPrice,
						content.DiscountedPrice,
						content.TotalSum,
						content.Comment))
					.ToList()))
			.AsSplitQuery()
			.ToListAsync(token);

		var buyers = await userDisplay.GetAsync(sales.Select(sale => sale.UserId).Distinct().ToArray(), token);
		var organizations = await organizationDisplay.GetAsync(
			sales.Select(sale => sale.OrganizationId).Distinct().ToArray(),
			token);
		var products = await productDisplay.GetAsync(
			sales.SelectMany(sale => sale.Contents).Select(content => content.ProductId).Distinct().ToArray(),
			token);
		var currencies = await currencyDisplay.GetAsync(
			sales.Select(sale => sale.CurrencyId).Distinct().ToArray(),
			token);

		var saleSchemas = sales
			.Select(sale =>
			{
				var currency = currencies.GetValueOrDefault(sale.CurrencyId);
				return (sale.OrganizationId, Schema: new SalesListDocumentSaleSchema
				{
					SaleDatetime = sale.SaleDatetime,
					CurrencyCode = currency?.Code ?? Missing,
					CurrencySign = currency?.Sign ?? Missing,
					StorageCode = sale.StorageCode,
					BuyerName = buyers.GetValueOrDefault(sale.UserId)?.FullName ?? Missing,
					TotalSum = sale.Contents.Sum(content => content.TotalSum),
					Comment = sale.Comment,
					Items = sale.Contents
						.Select(content => new SalesListDocumentItemSchema
						{
							Sku = products.GetValueOrDefault(content.ProductId)?.Sku ?? Missing,
							ProductName = products.GetValueOrDefault(content.ProductId)?.Name ?? Missing,
							Count = content.Count,
							OriginalPrice = content.OriginalPrice,
							DiscountedPrice = content.DiscountedPrice,
							TotalSum = content.TotalSum,
							Comment = content.Comment
						})
						.ToList()
				});
			})
			.ToList();

		var organizationSchemas = saleSchemas
			.GroupBy(sale => sale.OrganizationId)
			.Select(group =>
			{
				var organizationSales = group
					.Select(sale => sale.Schema)
					.OrderBy(sale => sale.CurrencyCode, StringComparer.Ordinal)
					.ThenBy(sale => sale.SaleDatetime)
					.ToList();
				return new SalesListDocumentOrganizationSchema
				{
					OrganizationName = organizations.GetValueOrDefault(group.Key)?.Name ?? Missing,
					SalesCount = organizationSales.Count,
					TotalsText = FormatTotals(BuildTotals(organizationSales), culture),
					Sales = organizationSales
				};
			})
			.OrderBy(organization => organization.OrganizationName, StringComparer.Create(culture, true))
			.ToList();

		var allSales = organizationSchemas.SelectMany(organization => organization.Sales).ToList();

		return new SalesListDocumentSchema
		{
			Period = FormatPeriod(request.RangeStartDate, request.RangeEndDate, culture),
			GeneratedAt = timeProvider.GetUtcNow().UtcDateTime,
			OrganizationsCount = organizationSchemas.Count,
			SalesCount = allSales.Count,
			ItemsCount = itemsCount,
			Totals = BuildTotals(allSales),
			Organizations = organizationSchemas
		};
	}

	private static IReadOnlyList<SalesListDocumentTotalSchema> BuildTotals(
		IEnumerable<SalesListDocumentSaleSchema> sales)
		=> sales
			.GroupBy(sale => (sale.CurrencyCode, sale.CurrencySign))
			.OrderBy(group => group.Key.CurrencyCode, StringComparer.Ordinal)
			.Select(group => new SalesListDocumentTotalSchema
			{
				CurrencyCode = group.Key.CurrencyCode,
				CurrencySign = group.Key.CurrencySign,
				SalesCount = group.Count(),
				ItemsCount = group.Sum(sale => sale.Items.Count),
				TotalSum = group.Sum(sale => sale.TotalSum)
			})
			.ToList();

	private static string FormatTotals(IEnumerable<SalesListDocumentTotalSchema> totals, CultureInfo culture)
		=> string.Join(
			" · ",
			totals.Select(total => $"{total.TotalSum.ToString("N2", culture)} {total.CurrencySign}"));

	private string FormatPeriod(DateTime? start, DateTime? end, CultureInfo culture)
		=> (start, end) switch
		{
			(null, null) => localizer.Get(SaleDocumentListPeriodAllTimeMessage.Instance, culture),
			({ } from, null) => $"{from.ToString("d", culture)} –",
			(null, { } to) => $"– {to.ToString("d", culture)}",
			({ } from, { } to) => $"{from.ToString("d", culture)} – {to.ToString("d", culture)}"
		};

	private sealed record SaleRow(
		DateTime SaleDatetime,
		Guid UserId,
		Guid OrganizationId,
		string StorageCode,
		int CurrencyId,
		string? Comment,
		IReadOnlyList<ContentRow> Contents);

	private sealed record ContentRow(
		int ProductId,
		int Count,
		decimal OriginalPrice,
		decimal DiscountedPrice,
		decimal TotalSum,
		string? Comment);
}
