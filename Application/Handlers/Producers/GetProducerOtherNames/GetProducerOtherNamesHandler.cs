using Application.Interfaces;
using Core.Dtos.Amw.Producers;
using Core.Interfaces.DbRepositories;
using Core.Models;
using FluentValidation;
using Mapster;

namespace Application.Handlers.Producers.GetProducerOtherNames;

public record GetProducerOtherNamesQuery(int ProducerId, PaginationModel Pagination) : IQuery<GetProducerOtherNamesResult>;
public record GetProducerOtherNamesResult(IEnumerable<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesHandler(IProducerRepository producerRepository) : IQueryHandler<GetProducerOtherNamesQuery, GetProducerOtherNamesResult>
{
    public async Task<GetProducerOtherNamesResult> Handle(GetProducerOtherNamesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result = await producerRepository.GetOtherNames(request.ProducerId, page, 
            size, false, cancellationToken);
        return new GetProducerOtherNamesResult(result.Adapt<List<ProducerOtherNameDto>>());
    }
}