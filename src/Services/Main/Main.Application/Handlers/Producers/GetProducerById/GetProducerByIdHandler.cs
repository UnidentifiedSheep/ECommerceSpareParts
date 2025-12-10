using Application.Common.Interfaces;
using Core.Attributes;
using Exceptions.Exceptions.Producers;
using Main.Core.Dtos.Anonymous.Producers;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Producers.GetProducerById;

[ExceptionType<ProducerNotFoundException>]
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