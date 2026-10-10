using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductGroups.GetProductGroups;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductGroupQueries
{
	[GraphQLName("byIds")]
	public async Task<IReadOnlyList<GqlProductGroup>> GetProductGroupsByIdsAsync(
		IProductGroupByIdDataLoader loader,
		IReadOnlyCollection<int> ids,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(ids, cancellationToken))
			.OfType<ProductGroupDto>()
			.Select(group => new GqlProductGroup(group))
			.ToList();

	[GraphQLName("search")]
	public async Task<IReadOnlyList<GqlProductGroup>> SearchAsync(
		GqlSearchProductGroupsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetProductGroupsQuery(
				input.SearchTerm,
				input.SortBy?.Select(sort => sort.ToSortExpression()).ToArray(),
				input.Pagination),
			cancellationToken);

		return result.Groups.Select(group => new GqlProductGroup(group)).ToArray();
	}
}
