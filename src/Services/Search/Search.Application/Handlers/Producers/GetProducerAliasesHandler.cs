using Application.Common.Interfaces.Cqrs;
using Search.Application.Dtos.Producers;
using Search.Application.Interfaces.Producer;
using Search.Application.Mapping;

namespace Search.Application.Handlers.Producers;

public record GetProducerAliasesQuery(int ProducerId) : IQuery<GetProducerAliasesResult>;

public record GetProducerAliasesResult(IEnumerable<ProducerAlias> Aliases);

public class GetProducerAliasesHandler(
    IProducerRepository producerRepository
) : IQueryHandler<GetProducerAliasesQuery, GetProducerAliasesResult>
{
    public async Task<GetProducerAliasesResult> Handle(
        GetProducerAliasesQuery request,
        CancellationToken cancellationToken)
    {
        var producer = await producerRepository.GetById(
            request.ProducerId,
            cancellationToken);

        return new GetProducerAliasesResult(
            producer?.Aliases.Select(x => x.ToProducerAliasDto()) ?? []);
    }
}