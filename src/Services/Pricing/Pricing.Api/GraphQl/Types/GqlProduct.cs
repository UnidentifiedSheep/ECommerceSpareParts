using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using MediatR;
using Pricing.Api.GraphQl.Types.Inputs;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Handlers.Pricing.GetRecommendedPrice;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("Product")]
public record GqlProduct
{
	[GraphQLName("id")]
	[Shareable]
	public int Id { get; }

	public GqlProduct(int id)
	{
		Id = id;
	}

	[GraphQLName("offers")]
	[RequireAllPermissions(PermissionCodes.PRICES_GET_DETAILED)]
	public async Task<IReadOnlyList<GqlPriceOption>> GetOffersAsync(
		GqlProductOffersInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetPriceOptionsForProductQuery(
				Id,
				input.CurrencyId,
				input.StorageCode,
				input.Sources,
				input.Pagination,
				input.SortBy.ToSortExpressions()),
			cancellationToken);

		return result.PriceOptions.Select(option => new GqlPriceOption(option)).ToArray();
	}

	[GraphQLName("recommendedPrice")]
	[RequireAnyPermission(PermissionCodes.PRICES_GET_STANDARD, PermissionCodes.PRICES_GET_DETAILED)]
	public async Task<GqlPriceRecommendation> GetRecommendedPriceAsync(
		GqlRecommendedPriceInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetRecommendedPriceQuery(
			Id, input.CurrencyId, input.StorageCode, input.Quantity, input.Sources), cancellationToken);
		return new GqlPriceRecommendation(result.Item);
	}
}
