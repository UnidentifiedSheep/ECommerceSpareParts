using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Characteristics.GetCharacteristics;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class CharacteristicQueries
{
	[GraphQLName("byIds")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_DETAILS_GET)]
	public async Task<IReadOnlyList<GqlCharacteristic>> GetCharacteristicsByIdsAsync(
		ICharacteristicByIdDataLoader loader,
		IReadOnlyCollection<int> ids,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(ids, cancellationToken))
			.OfType<CharacteristicDto>()
			.Select(characteristic => new GqlCharacteristic(characteristic))
			.ToList();

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_DETAILS_GET)]
	public async Task<IReadOnlyList<GqlCharacteristic>> SearchAsync(
		GqlSearchCharacteristicsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetCharacteristicsQuery(
				input.SearchTerm,
				input.SortBy.ToSortExpressions(),
				input.Pagination),
			cancellationToken);

		return result.Characteristics.Select(characteristic => new GqlCharacteristic(characteristic)).ToArray();
	}
}
