using GreenDonut;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Handlers.ProducerAliases;
using Main.Application.Handlers.Producers;
using Main.Application.Handlers.ProducerSupplierMappings;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class ProducerDataLoaders
{
	[DataLoader]
	public static async Task<Dictionary<int, ProducerDto>> GetProducerByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
	{
		return (await sender.Send(new GetProducersByIdsQuery(keys), cancellationToken)).Producers
			.ToDictionary(x => x.Key, x => x.Value);
	}

	[DataLoader]
	public static async Task<Dictionary<int, List<string>>> GetProducerAliasesByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
	{
		return (await sender.Send(new GetProducersAliasesQuery(keys), cancellationToken)).ProducersAliases;
	}

	[DataLoader]
	public static async Task<Dictionary<int, List<ProducerSupplierMappingDto>>>
		GetProducerSupplierMappingsByIdAsync(
			IReadOnlyList<int> keys,
			ISender sender,
			CancellationToken cancellationToken)
	{
		return (await sender.Send(new GetProducersSupplierMappingsQuery(keys), cancellationToken)).Mappings;
	}
}
