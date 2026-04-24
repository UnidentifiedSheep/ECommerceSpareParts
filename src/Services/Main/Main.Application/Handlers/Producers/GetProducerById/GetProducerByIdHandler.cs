using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer;
using Main.Entities.Exceptions.Producers;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetProducerById;

public record GetProducerByIdQuery(int Id) : IQuery<GetProducerByIdResult>;

public record GetProducerByIdResult(ProducerDto Producer);

public class GetProducerByIdHandler(IReadRepository<Producer, int> repository)
    : IQueryHandler<GetProducerByIdQuery, GetProducerByIdResult>
{
    public async Task<GetProducerByIdResult> Handle(GetProducerByIdQuery request, CancellationToken cancellationToken)
    {
        var producer = await repository.Query
            .Select(x => new ProducerDto
            {
                Id = x.Id,
                Description = x.Description,
                Name = x.Name
            }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken) 
                       ?? throw new ProducerNotFoundException(request.Id);
        return new GetProducerByIdResult(producer);
    }
}