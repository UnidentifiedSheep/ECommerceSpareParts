using HotChocolate;
using MediatR;
using Search.Api.GraphQl.Types;
using Search.Api.GraphQl.Types.Inputs;
using Search.Application.Handlers.Producers.SearchProducers;

namespace Search.Api.GraphQl.Queries;

public sealed class ProducerQueries
{
	[GraphQLName("search")]
	public async Task<List<GqlProducer>> SearchProducersAsync(
		ISender sender,
		GqlProducerSearchInput input,
		CancellationToken ct)
	{
		var result = await sender.Send(new SearchProducersQuery(input.Query, input.Pagination), ct);

		return result.Producers.Select(x => new GqlProducer(x.Id)).ToList();
	}
}
