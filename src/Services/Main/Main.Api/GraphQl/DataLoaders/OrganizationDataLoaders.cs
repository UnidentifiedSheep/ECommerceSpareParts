using GreenDonut;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class OrganizationDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, OrganizationDto>> GetOrganizationByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetOrganizationsByIdsQuery(keys), cancellationToken)).Organizations;
}
