using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Services.Document;
using Main.Entities;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver,
	IS3Service s3Service,
	IReadRepository<Sale, Guid> repository,
	ILocalizer localizer,
	IOptions<S3BucketsOptions> options
	) : DocumentDefinitionBase<
	SingleSaleDocumentRequest,
	SingleSaleDocumentSchema>(templateResolver, s3Service, options)
{
	private static readonly DocumentTypePair[] SupportedTypes =
	[
		new(DocumentType.Excel, DocumentType.Excel)
	];

	public override string SystemName => "SingleSale";
	public override string DocumentGroup => "Sales";
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

		return await repository
			.Query
			.Select(x => new SingleSaleDocumentSchema
			{
				Id = x.Id,
				BuyerName = x.User.UserInfo == null
					? "—"
					: x.User.UserInfo.Name + " " + x.User.UserInfo.Surname,
				Comment = x.Comment,
				CurrencyCode = x.Currency.Code,
				CurrencySign = x.Currency.CurrencySign,
				OrganizationName = x.Organization.Name,
				SaleDatetime = x.SaleDatetime,
				State = x.State == SaleState.Draft ? draft :
					x.State == SaleState.Completed ? completed :
					x.State == SaleState.Deleted ? deleted : unknown,
				StorageCode = x.StorageCode,
				Items = x
					.Contents
					.Select(z => new SingleSaleDocumentItemSchema
					{
						Comment = z.Comment,
						Count = z.Count,
						Discount = z.Discount,
						Price = z.Price,
						ProductId = z.ProductId,
						ProductName = z.Product.Name,
						Sku = z.Product.Sku,
						TotalSum = z.TotalSum
					})
					.ToList(),
				TransactionId = x.TransactionId,
				TotalSum = x.Transaction.Amount
			})
			.FirstOrDefaultAsync(x => x.Id == saleId, token)
			?? throw new SaleNotFoundException(saleId);
	}
}
