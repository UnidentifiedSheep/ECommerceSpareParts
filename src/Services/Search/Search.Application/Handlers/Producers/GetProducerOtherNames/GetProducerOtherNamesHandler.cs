using Application.Common.Interfaces.Cqrs;
using Search.Application.Dtos.Producers;
using Search.Application.Interfaces.Producer;
using Search.Application.Mapping;

namespace Search.Application.Handlers.Producers.GetProducerOtherNames;

public record GetProducerOtherNamesQuery(int ProducerId) : IQuery<GetProducerOtherNamesResult>;

public record GetProducerOtherNamesResult(IEnumerable<ProducerOtherNameDto> OtherNames);

public class GetProducerOtherNamesHandler(
    IProducerRepository producerRepository) : IQueryHandler<GetProducerOtherNamesQuery, GetProducerOtherNamesResult>
{
    public async Task<GetProducerOtherNamesResult> Handle(
        GetProducerOtherNamesQuery request,
        CancellationToken cancellationToken)
    {
        var producer = await producerRepository.GetById(
            request.ProducerId,
            cancellationToken);

        return new GetProducerOtherNamesResult(
            producer?.OtherNames.Select(x => x.ToProducerOtherNameDto()) ?? []);
    }
}
