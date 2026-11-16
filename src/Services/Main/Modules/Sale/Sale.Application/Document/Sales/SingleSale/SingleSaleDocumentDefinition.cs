using Application.Common.Interfaces.Repositories;
using Enums;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Document.Document;
using Main.Application.Document.Interfaces.Document;
using Main.Entities.Sale;
using Main.Entities.Sale.Enums;
using Main.Entities.Sale.Exceptions;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Options;

namespace Main.Modules.Sale.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver,
	IS3Service s3Service,
	IReadRepository<Entities.Sale.Sale.Sale, Guid> repository,
	IUserNames userNames,
	ICurrencyDisplay currencyDisplay,
	IOrganizationNames organizationNames,
	IProductNames productNames,
	ILocalizer localizer,
	IOptions<S3BucketsOptions> options
) : DocumentDefinitionBase<
	SingleSaleDocumentRequest,
	SingleSaleDocumentSchema>(
	templateResolver,
	s3Service,
	options)
{
	private static readonly DocumentTypePair[] SupportedTypes =
	[
		new(DocumentType.Excel, DocumentType.Excel)
	];

	public override string SystemName => "SingleSale";
	public override string DocumentGroup => "Sales";
	public override IReadOnlyCollection<PermissionCodes> RequiredPermissions => [PermissionCodes.SALES_GET];
	public override ILocalizableMessage Name => SaleDocumentSingleNameMessage.Instance;
	public override ILocalizableMessage Description => SaleDocumentSingleDescriptionMessage.Instance;
	protected override DocumentTypePair[] SupportedTypePairs => SupportedTypes;

	protected override async Task<SingleSaleDocumentSchema> GetSchemaDataAsync(
		SingleSaleDocumentRequest request,
		CancellationToken token)
	{
		var saleId = request.SaleId;
		var culture = DocumentCulture.GetRequired(request.Culture);
		var draft = localizer.Get(SaleStateDraftMessage.Instance, culture);
		var completed = localizer.Get(SaleStateCompletedMessage.Instance, culture);
		var deleted = localizer.Get(SaleStateDeletedMessage.Instance, culture);
		var unknown = localizer.Get(SaleStateUnknownMessage.Instance, culture);

		var sale = await repository
				.Query
				.AsNoTracking()
				.Include(x => x.Contents)
				.AsSplitQuery()
				.FirstOrDefaultAsync(x => x.Id == saleId, token)
			?? throw new SaleNotFoundException(saleId);

		var buyer = (await userNames.GetFullNamesAsync([sale.UserId], token)).GetValueOrDefault(sale.UserId);
		var currency = await currencyDisplay.GetAsync(sale.CurrencyId, token);
		var organization = (await organizationNames.GetAsync([sale.OrganizationId], token))
			.GetValueOrDefault(sale.OrganizationId);
		var products = await productNames.GetAsync(
			sale.Contents.Select(content => content.ProductId).Distinct().ToArray(),
			token);

		return new SingleSaleDocumentSchema
		{
			Id = sale.Id,
			BuyerName = buyer ?? "—",
			Comment = sale.Comment,
			CurrencyCode = currency?.Code!,
			CurrencySign = currency?.Sign!,
			OrganizationName = organization!,
			SaleDatetime = sale.SaleDatetime,
			State = sale.State == SaleState.Draft ? draft :
				sale.State == SaleState.Completed ? completed :
				sale.State == SaleState.Deleted ? deleted : unknown,
			StorageCode = sale.StorageCode,
			Items = sale.Contents
				.Select(content => new SingleSaleDocumentItemSchema
				{
					Comment = content.Comment,
					Count = content.Count,
					OriginalPrice = content.OriginalPrice,
					DiscountedPrice = content.DiscountedPrice,
					ProductId = content.ProductId,
					ProductName = products.GetValueOrDefault(content.ProductId)?.Name!,
					Sku = products.GetValueOrDefault(content.ProductId)?.Sku!,
					TotalSum = content.TotalSum
				})
				.ToList(),
			TransactionId = sale.TransactionId,
			TotalSum = sale.Contents.Sum(content => content.TotalSum)
		};
	}
}
