using Application.Common.Interfaces;
using Exceptions.Exceptions.Producers;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Producers.GetProducerById;

public record GetProducerByIdQuery(int Id) : IQuery<GetProducerByIdResult>;
public record GetProducerByIdResult(ProducerDto Producer);

public class GetProducerByIdHandler(IProducerRepository producerRepository) : IQueryHandler<GetProducerByIdQuery, GetProducerByIdResult>
{
    public async Task<GetProducerByIdResult> Handle(GetProducerByIdQuery request, CancellationToken cancellationToken)
    {
        var producer = await producerRepository.GetProducer(request.Id, false, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.Id);
        return new GetProducerByIdResult(producer.Adapt<ProducerDto>());
    }
}