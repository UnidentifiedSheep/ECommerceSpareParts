using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using MediatR;
using Pricing.Api.GraphQl.Types.Inputs;
using Pricing.Application.Handlers.Pricing;

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
}
