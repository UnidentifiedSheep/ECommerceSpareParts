using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.GetProducerOtherNames;

public record GetProducerOtherNamesQuery(int ProducerId, int Page, int ViewCount) : IQuery<GetProducerOtherNamesResult>;
public record GetProducerOtherNamesResult(IEnumerable<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesValidation : AbstractValidator<GetProducerOtherNamesQuery>
{
    public GetProducerOtherNamesValidation()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetProducerOtherNamesHandler(DContext context) : IQueryHandler<GetProducerOtherNamesQuery, GetProducerOtherNamesResult>
{
    public async Task<GetProducerOtherNamesResult> Handle(GetProducerOtherNamesQuery request, CancellationToken cancellationToken)
    {
        _ = await context.Producers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProducerId, cancellationToken)
            ?? throw new ProducerNotFoundException(request.ProducerId);

        var names = await context.ProducersOtherNames
            .AsNoTracking()
            .Where(x => x.ProducerId == request.ProducerId)
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken);
        
        return new GetProducerOtherNamesResult(names.Adapt<List<ProducerOtherNameDto>>());
    }
}