using GreenDonut;
using Main.Modules.Product.Application.Dtos.Producer;
using Main.Modules.Product.Application.Dtos.Producer.SupplierMappings;
using Main.Modules.Product.Application.Queries.ProducerAliases;
using Main.Modules.Product.Application.Queries.ProducerSupplierMappings;
using Main.Modules.Product.Application.Queries.Producers;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class ProducerDataLoaders
{
	[DataLoader]
	public static async Task<Dictionary<int, ProducerDto>> GetProducerByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetProducersByIdsQuery(keys), cancellationToken)).Producers
			.ToDictionary(x => x.Key, x => x.Value);

	[DataLoader]
	public static async Task<Dictionary<int, List<string>>> GetProducerAliasesByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetProducersAliasesQuery(keys), cancellationToken)).ProducersAliases;

	[DataLoader]
	public static async Task<Dictionary<int, List<ProducerSupplierMappingDto>>>
		GetProducerSupplierMappingsByIdAsync(
			IReadOnlyList<int> keys,
			ISender sender,
			CancellationToken cancellationToken)
		=> (await sender.Send(
			new GetProducersSupplierMappingsQuery(keys),
			cancellationToken)).Mappings;
}
