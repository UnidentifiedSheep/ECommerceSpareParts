using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Characteristics;

public sealed record GetCharacteristicsByIdsQuery : IQuery<GetCharacteristicsByIdsResult>
{
	public IReadOnlyList<int> Ids { get; }

	public GetCharacteristicsByIdsQuery(int id) : this([id])
	{
	}

	public GetCharacteristicsByIdsQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

}

public sealed record GetCharacteristicsByIdsResult(IReadOnlyDictionary<int, CharacteristicDto> Characteristics);

public sealed class GetCharacteristicsByIdsHandler(
	IReadRepository<Characteristic, int> repository,
	IProjectionProvider<Characteristic, CharacteristicDto> projection)
	: IQueryHandler<GetCharacteristicsByIdsQuery, GetCharacteristicsByIdsResult>
{
	public async Task<GetCharacteristicsByIdsResult> Handle(
		GetCharacteristicsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetCharacteristicsByIdsResult(new Dictionary<int, CharacteristicDto>());

		var characteristics = await repository.Query
			.Where(x => request.Ids.Contains(x.Id))
			.Project(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		return new GetCharacteristicsByIdsResult(characteristics);
	}
}
