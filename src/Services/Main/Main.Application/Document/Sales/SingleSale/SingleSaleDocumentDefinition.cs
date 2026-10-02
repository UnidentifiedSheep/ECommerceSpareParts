using Application.Common.Interfaces.Repositories;
using Main.Application.Handlers.Sales;
using Main.Application.Interfaces.Services.Document;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver,
	IReadRepository<Sale, Guid> repository
	) : DocumentDefinitionBase<SingleSaleDocumentRequest, DocumentResponse>(templateResolver)
{
	private static readonly DocumentTypePair[] SupportedTypes = [new(DocumentType.Excel, DocumentType.Excel)];

	public override string SystemName => "SingleSale";
	public override Type SchemaType => typeof(SingleSaleDocumentSchema);
	protected override string BaseTemplateKey => "Sales/SingleSale/Current";
	protected override DocumentTypePair[] SupportedTypePairs => SupportedTypes;

	public override async Task<DocumentResponse> GenerateAsync(
		SingleSaleDocumentRequest request,
		Stream destination,
		CancellationToken token = default)
	{
		var template = await GetTemplateAsync(request, token);
		var data = await GetSchemaDataAsync(request.SaleId, token);
	}

	private async Task<SingleSaleDocumentSchema> GetSchemaDataAsync(Guid saleId, CancellationToken token)
	{
		return await repository
			.Query
			.Select(x => new SingleSaleDocumentSchema
			{
				Id = x.Id,
				BuyerName = x.User.UserInfo == null
					? ""
					: x.User.UserInfo.Name + " " + x.User.UserInfo.Surname,
				Comment = x.Comment,
				CurrencyCode = x.Currency.Code,
				CurrencySign = x.Currency.CurrencySign,
				OrganizationName = x.Organization.Name,
				SaleDatetime = x.SaleDatetime,
				State = x.State,
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
