using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.GetProducers;

public record GetProducersQuery(string? SearchTerm, int ViewCount, int Page) : IQuery<GetProducersResult>;
public record GetProducersResult(IEnumerable<ProducerDto> Producers);

public class GetProducersValidation : AbstractValidator<GetProducersQuery>
{
    public GetProducersValidation()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetProducersHandler(DContext context) : IQueryHandler<GetProducersQuery, GetProducersResult>
{
    public async Task<GetProducersResult> Handle(GetProducersQuery request, CancellationToken cancellationToken)
    {
        var result = await context.Producers.AsNoTracking()
            .Where(x => string.IsNullOrEmpty(request.SearchTerm) || 
                        EF.Functions.ILike(x.Name, $"%{request.SearchTerm}%"))
            .OrderBy(x => x.Id)
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetProducersResult(result.Adapt<List<ProducerDto>>());
    }
}