using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Product;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using MediatR;
using Pricing.Api.GraphQl.DataLoaders;
using Pricing.Api.GraphQl.Types;
using Pricing.Api.GraphQl.Types.Inputs;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Handlers.Pricing.GetRecommendedPrice;

namespace Main.Api.GraphQl.Pricing;

[ExtendObjectType<GqlProduct>]
public sealed class GqlProductPricingExtensions
{
	[GraphQLName("offers")]
	[RequireAllPermissions(PermissionCodes.PRICES_GET_DETAILED)]
	public async Task<IReadOnlyList<GqlPriceOption>> GetOffersAsync(
		[Parent] GqlProduct product,
		GqlProductOffersInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetPriceOptionsForProductQuery(
				product.Id,
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
		[Parent] GqlProduct product,
		GqlRecommendedPriceInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetRecommendedPriceQuery(
				product.Id,
				input.CurrencyId,
				input.StorageCode,
				input.Quantity,
				input.Sources),
			cancellationToken);
		return new GqlPriceRecommendation(result.Item);
	}
}

[ExtendObjectType<GqlPriceOffer>]
public sealed class GqlPriceOfferExtensions
{
	[GraphQLName("product")]
	public GqlProduct GetProduct([Parent] GqlPriceOffer offer) => new(offer.ProductId);

	[GraphQLName("currency")]
	public Task<GqlCurrency> GetCurrencyAsync(
		[Parent] GqlPriceOffer offer,
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> PricingCurrency.LoadAsync(offer.CurrencyId, loader, cancellationToken);
}

[ExtendObjectType<GqlPriceOption>]
public sealed class GqlPriceOptionExtensions
{
	[GraphQLName("currency")]
	public Task<GqlCurrency> GetCurrencyAsync(
		[Parent] GqlPriceOption option,
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> PricingCurrency.LoadAsync(option.CurrencyId, loader, cancellationToken);
}

[ExtendObjectType<GqlPriceRecommendation>]
public sealed class GqlPriceRecommendationExtensions
{
	[GraphQLName("currency")]
	public Task<GqlCurrency> GetCurrencyAsync(
		[Parent] GqlPriceRecommendation recommendation,
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> PricingCurrency.LoadAsync(recommendation.CurrencyId, loader, cancellationToken);
}

[ExtendObjectType<GqlMarkupGroup>]
public sealed class GqlMarkupGroupExtensions
{
	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		[Parent] GqlMarkupGroup group,
		IMarkupGroupByIdDataLoader groupLoader,
		ICurrencyByIdDataLoader currencyLoader,
		CancellationToken cancellationToken)
		=> await PricingCurrency.LoadAsync(
			await group.GetCurrencyIdAsync(groupLoader, cancellationToken),
			currencyLoader,
			cancellationToken);
}

internal static class PricingCurrency
{
	public static async Task<GqlCurrency> LoadAsync(
		int currencyId,
		ICurrencyByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> new(await loader.LoadAsync(currencyId, cancellationToken)
			?? throw new CurrencyNotFoundException(currencyId));
}
