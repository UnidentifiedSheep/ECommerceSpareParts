using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver,
	IS3Service s3Service,
	IReadRepository<Sale, Guid> repository,
	IOptions<S3BucketsOptions> options
	) : DocumentDefinitionBase<
	SingleSaleDocumentRequest,
	DocumentResponse,
	SingleSaleDocumentSchema>(templateResolver)
{
	private static readonly DocumentTypePair[] SupportedTypes =
	[
		new(DocumentType.Excel, DocumentType.Excel)
	];

	public override string SystemName => "SingleSale";
	public override string DocumentGroup => "Sales";
	protected override DocumentTypePair[] SupportedTypePairs => SupportedTypes;

	public override async Task<DocumentResponse> GenerateAsync(
		SingleSaleDocumentRequest request,
		CancellationToken token = default)
	{
		using var template = await GetTemplateAsync(request, token);
		var data = await GetSchemaDataAsync(request.SaleId, token);
		AddAllFields(template, data);

		await using var output = new FileStream(
			Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
			new FileStreamOptions
			{
				Mode = FileMode.CreateNew,
				Access = FileAccess.ReadWrite,
				Share = FileShare.None,
				Options = FileOptions.DeleteOnClose
			});

		await template.RenderAsync(output, token);
		output.Position = 0;

		var generatedAt = DateTimeOffset.UtcNow;
		var key = $"Generated/{DocumentGroup}/{SystemName}/" +
		          $"{generatedAt:yyyy/MM/dd}/{request.SaleId:N}-{Guid.NewGuid():N}" +
		          template.Type.GetFileExtension();
		var bucket = options.Value.Documents;
		var uploadedKey = await s3Service.UploadFileAsync(
			bucket.Name,
			output,
			key,
			template.Type.GetContentType());

		return new DocumentResponse
		{
			GeneratedFileLink = $"{bucket.PublicBaseUrl.TrimEnd('/')}/{uploadedKey}"
		};
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
