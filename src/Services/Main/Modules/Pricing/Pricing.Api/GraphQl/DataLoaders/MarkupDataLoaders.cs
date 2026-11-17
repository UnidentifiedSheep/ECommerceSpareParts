using GreenDonut;
using MediatR;
using Pricing.Application.Dtos.Markup;
using Pricing.Application.Handlers.Markup;

namespace Pricing.Api.GraphQl.DataLoaders;

public static class MarkupDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<int, MarkupGroupDto>> GetMarkupGroupByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetMarkupGroupsByIdsQuery(keys), cancellationToken)).Groups;
}
