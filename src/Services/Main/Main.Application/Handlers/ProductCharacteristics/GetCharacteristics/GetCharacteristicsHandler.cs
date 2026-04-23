using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Anonymous.Articles;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;

public record GetCharacteristicsQuery(int ProductId, PaginationModel Pagination)
    : IQuery<GetCharacteristicsResult>;

public record GetCharacteristicsResult(IReadOnlyList<ProductCharacteristicDto> Characteristics);

public class GetCharacteristicsHandler(
    IReadRepository<ProductCharacteristic, (int, string)> repository)
    : IQueryHandler<GetCharacteristicsQuery, GetCharacteristicsResult>
{
    public async Task<GetCharacteristicsResult> Handle(
        GetCharacteristicsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.ProductId == request.ProductId)
            .Select(x => new ProductCharacteristicDto
            {
                ProductId = x.ProductId,
                Name = x.Name,
                Value = x.Value,
            })
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetCharacteristicsResult(result);
    }
}