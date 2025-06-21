using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Sales.GetSales;

public record GetSalesQuery(DateTime RangeStartDate, DateTime RangeEndDate, int Page, int ViewCount, string? BuyerId, 
    int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesValidation : AbstractValidator<GetSalesQuery>
{
    public GetSalesValidation()
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetSalesHandler(DContext context) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Sales.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.BuyerId))
        {
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BuyerId, cancellationToken) ?? throw new UserNotFoundException(request.BuyerId);
            query = query.Where(x => x.BuyerId == request.BuyerId);
        }

        if (request.CurrencyId != null)
        {
            _ = await context.Currencies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CurrencyId, cancellationToken: cancellationToken) ?? throw new CurrencyNotFoundException(request.CurrencyId);
            query = query.Where(x => x.CurrencyId == request.CurrencyId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            var normalizedSearchTerm = request.SearchTerm.ToNormalizedArticleNumber();
            query = query.Where(x => x.SaleContents
                .Any(content => (EF.Functions.ToTsVector("russian", content.Article.ArticleName)
                                     .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                 EF.Functions.ILike(content.Article.NormalizedArticleNumber, $"%{normalizedSearchTerm}%"))) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")));
        }
        
        var startDate = DateTime.SpecifyKind(request.RangeStartDate.Date, DateTimeKind.Unspecified);
        var endDate = DateTime.SpecifyKind(request.RangeEndDate.Date, DateTimeKind.Unspecified).AddDays(1);
        var result = await query.Where(x => (x.CreationDatetime >= startDate && x.CreationDatetime <= endDate))
            .Include(x => x.Transaction)
            .Include(x => x.Buyer)
            .SortBy(request.SortBy)
            .Skip(request.ViewCount * request.Page)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken);

        return new GetSalesResult(result.Adapt<List<SaleDto>>());
    }
}