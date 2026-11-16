using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Producer;
using Main.Api.GraphQl.Types.Inputs.Producer;
using Main.Modules.Product.Application.Dtos.Producer;
using Main.Modules.Product.Application.Handlers.Producers.GetProducers;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class ProducerQueries
{
	[GraphQLName("byId")]
	[Lookup]
	public async Task<GqlProducer?> GetProducerByIdAsync(
		IProducerByIdDataLoader loader,
		int id,
		CancellationToken ct)
	{
		var producer = await loader.LoadAsync(id, ct);
		return producer is null ? null : new GqlProducer(producer);
	}

	[GraphQLName("byIds")]
	public async Task<IReadOnlyList<GqlProducer>> GetProducersByIdsAsync(
		IProducerByIdDataLoader loader,
		IReadOnlyCollection<int> ids,
		CancellationToken ct)
		=> (await loader.LoadAsync(ids, ct))
			.OfType<ProducerDto>()
			.Select(x => new GqlProducer(x))
			.ToList();

	[GraphQLName("list")]
	public async Task<IReadOnlyList<GqlProducer>> ListAsync(
		GqlGetProducersInput input,
		ISender sender,
		CancellationToken ct)
		=> (await sender.Send(
				new GetProducersQuery(input.SearchTerm, input.Ids ?? [], input.Pagination),
				ct))
			.Producers
			.Select(producer => new GqlProducer(producer))
			.ToArray();
}
