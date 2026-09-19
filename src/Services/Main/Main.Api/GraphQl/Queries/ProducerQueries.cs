using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Producer;
using Main.Application.Dtos.Producer;

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
		CancellationToken ct) => (await loader.LoadAsync(ids, ct))
		.OfType<ProducerDto>()
		.Select(x => new GqlProducer(x))
		.ToList();
}
