using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers;

public record GetFullProducersQuery : IQuery<GetFullProducersResult>
{
	public GetFullProducersQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToList();
	}

	public GetFullProducersQuery(int id) : this([id])
	{
	}

	public IReadOnlyList<int> Ids { get; }
}

public record GetFullProducersResult(IReadOnlyList<ProducerFullDto> Producers);

public class GetFullProducersHandler(
	IReadRepository<Producer, int> repository,
	IProjectionProvider<Producer, ProducerFullDto> projection)
	: IQueryHandler<GetFullProducersQuery, GetFullProducersResult>
{
	public async Task<GetFullProducersResult> Handle(
		GetFullProducersQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetFullProducersResult([]);

		var result = await repository
			.Query
			.Where(x => request.Ids.Contains(x.Id))
			.Project(projection)
			.ToListAsync(cancellationToken);

		return new GetFullProducersResult(result);
	}
}
