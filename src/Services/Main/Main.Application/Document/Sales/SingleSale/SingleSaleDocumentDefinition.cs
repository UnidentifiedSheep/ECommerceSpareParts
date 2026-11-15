using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Enums;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Services.Document;
using Main.Entities;
using Main.Entities.Exceptions;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Entities.User;
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
	IReadRepository<User, Guid> userRepository,
	IReadRepository<Currency, int> currencyRepository,
	IReadRepository<Organization, Guid> organizationRepository,
	IReadRepository<Product, int> productRepository,
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

		return await repository
				.Query
				.Select(x => new SingleSaleDocumentSchema
				{
					Id = x.Id,
					BuyerName = userRepository.Query
						.Where(user => user.Id == x.UserId)
						.Select(user => user.UserInfo == null
							? "—"
							: user.UserInfo.Name + " " + user.UserInfo.Surname)
						.FirstOrDefault()!,
					Comment = x.Comment,
					CurrencyCode = currencyRepository.Query
						.Where(currency => currency.Id == x.CurrencyId)
						.Select(currency => currency.Code)
						.FirstOrDefault()!,
					CurrencySign = currencyRepository.Query
						.Where(currency => currency.Id == x.CurrencyId)
						.Select(currency => currency.CurrencySign)
						.FirstOrDefault()!,
					OrganizationName = organizationRepository.Query
						.Where(organization => organization.Id == x.OrganizationId)
						.Select(organization => organization.Name)
						.FirstOrDefault()!,
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
							OriginalPrice = z.OriginalPrice,
							DiscountedPrice = z.DiscountedPrice,
							ProductId = z.ProductId,
							ProductName = productRepository.Query
								.Where(product => product.Id == z.ProductId)
								.Select(product => product.Name)
								.FirstOrDefault()!,
							Sku = productRepository.Query
								.Where(product => product.Id == z.ProductId)
								.Select(product => product.Sku)
								.FirstOrDefault()!,
							TotalSum = z.TotalSum
						})
						.ToList(),
					TransactionId = x.TransactionId,
					TotalSum = x.Contents.Sum(content => content.TotalSum)
				})
				.FirstOrDefaultAsync(x => x.Id == saleId, token)
			?? throw new SaleNotFoundException(saleId);
	}
}
