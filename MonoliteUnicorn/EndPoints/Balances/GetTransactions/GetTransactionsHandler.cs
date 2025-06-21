using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Balances;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Balances.GetTransactions;

public record GetTransactionsQuery(DateTime RangeStart, DateTime RangeEnd, 
    int? CurrencyId, string? SenderId, string? ReceiverId, int Page, int ViewCount) : IQuery<GetTransactionsResult>;
public record GetTransactionsResult(IEnumerable<TransactionDto> Transactions);

public class GetTransactionsValidation : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidation()
    {
        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != x.SenderId || string.IsNullOrWhiteSpace(x.ReceiverId))
            .WithMessage("Отправитель и получатель не могут быть одинаковы");

        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => !string.IsNullOrWhiteSpace(x.ReceiverId) || !string.IsNullOrWhiteSpace(x.SenderId))
            .WithMessage("Должен быть указан хотя бы один: отправитель или получатель");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart.Date <= x.RangeEnd.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeEnd.Date <= x.RangeStart.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}


public class GetTransactionsHandler(DContext context) : IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
    public async Task<GetTransactionsResult> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Transactions.AsNoTracking()
            .Where(x => (request.RangeStart <= x.TransactionDatetime && request.RangeEnd >= x.TransactionDatetime) &&
                        (request.CurrencyId == null || x.CurrencyId == request.CurrencyId));
        if (!string.IsNullOrWhiteSpace(request.SenderId) && !string.IsNullOrWhiteSpace(request.ReceiverId))
            query = query.Where(x => (x.SenderId == request.SenderId && x.ReceiverId == request.ReceiverId) || (x.SenderId == request.ReceiverId && x.ReceiverId == request.SenderId));
        else
            query = query.Where(x => x.SenderId == request.SenderId || x.ReceiverId == request.SenderId && string.IsNullOrWhiteSpace(request.SenderId))
                .Where(x => x.SenderId == request.ReceiverId || x.ReceiverId == request.ReceiverId && string.IsNullOrWhiteSpace(request.ReceiverId));
        
        
        query = query.OrderBy(x => new {x.TransactionDatetime, x.Id})
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount);
        var result = await query.ToListAsync(cancellationToken);
        return new GetTransactionsResult(result.Adapt<List<TransactionDto>>());
    }
}