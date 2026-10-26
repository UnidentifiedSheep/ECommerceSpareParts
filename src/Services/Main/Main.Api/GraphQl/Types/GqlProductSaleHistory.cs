using HotChocolate;
using Main.Api.GraphQl.Types.Product;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Currencies;
using MediatR;

namespace Main.Api.GraphQl.Types;

[GraphQLName("ProductSaleHistory")]
public sealed record GqlProductSaleHistory(
	[property: GraphQLIgnore] ProductSaleHistoryDto History)
{
	[GraphQLName("saleContentId")]
	public int SaleContentId => History.SaleContentId;

	[GraphQLName("product")]
	public GqlProduct Product => new(History.ProductId);

	[GraphQLName("organization")]
	public GqlOrganization Organization => new(History.OrganizationId);

	[GraphQLName("storage")]
	public GqlStorage Storage => new(History.StorageCode);

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(ISender sender, CancellationToken cancellationToken) =>
		new((await sender.Send(new GetCurrencyByIdQuery(History.CurrencyId), cancellationToken)).Currency);

	[GraphQLName("quantity")]
	public int Quantity => History.Quantity;

	[GraphQLName("discount")]
	public decimal Discount => History.Discount;

	[GraphQLName("price")]
	public decimal Price => History.Price;

	[GraphQLName("averageBuyPrice")]
	public decimal AverageBuyPrice => History.AverageBuyPrice;

	[GraphQLName("saleDate")]
	public DateTime SaleDate => History.SaleDate;

	[GraphQLName("createdBy")]
	public GqlUser? CreatedBy => History.WhoCreated.HasValue ? new GqlUser(History.WhoCreated.Value) : null;
}
