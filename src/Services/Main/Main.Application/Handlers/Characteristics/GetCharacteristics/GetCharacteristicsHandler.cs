using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Characteristics.GetCharacteristics;

public record GetCharacteristicsQuery(
	string? SearchTerm,
	IReadOnlyCollection<string>? SortBy,
	Pagination Pagination) : IQuery<GetCharacteristicsResult>;

public record GetCharacteristicsResult(IReadOnlyList<CharacteristicDto> Characteristics);

public class GetCharacteristicsHandler(
	IReadRepository<Characteristic, int> repository,
	IProjectionProvider<Characteristic, CharacteristicDto> projection)
	: IQueryHandler<GetCharacteristicsQuery, GetCharacteristicsResult>
{
	public async Task<GetCharacteristicsResult> Handle(
		GetCharacteristicsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;
		var searchTerm = request.SearchTerm?.Trim();
		if (!string.IsNullOrWhiteSpace(searchTerm))
			query = query.Where(characteristic => EF.Functions.ILike(characteristic.Name, $"%{searchTerm}%"));

		var characteristics = await query
			.SortBy(request.SortBy)
			.ThenBy(characteristic => characteristic.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetCharacteristicsResult(characteristics);
	}
}
