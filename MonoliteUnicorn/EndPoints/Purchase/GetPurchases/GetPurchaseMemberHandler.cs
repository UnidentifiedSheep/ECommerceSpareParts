using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Member.Purchase;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Purchase.GetPurchases;

public record GetPurchaseMemberQuery(DateTime RangeStartDate, DateTime RangeEndDate, int Page, int ViewCount, string UserId, 
    int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetPurchaseMemberResult>;
public record GetPurchaseMemberResult(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesMemberValidation : AbstractValidator<GetPurchaseMemberQuery>
{
    public GetPurchasesMemberValidation()
    {
        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.Start.Date <= x.End.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(query => new { Start = query.RangeStartDate, End = query.RangeEndDate })
            .Must(x => x.End.Date <= x.Start.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetPurchaseMemberHandler(DContext context) : IQueryHandler<GetPurchaseMemberQuery, GetPurchaseMemberResult>
{
    public async Task<GetPurchaseMemberResult> Handle(GetPurchaseMemberQuery request, CancellationToken cancellationToken)
    {
        var query = context.Sales.AsNoTracking().Where(x => x.BuyerId == request.UserId);

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
                                 EF.Functions.ILike(content.Article.NormalizedArticleNumber, $"%{normalizedSearchTerm}%")) || 
                                (content.Comment != null && EF.Functions.ILike(content.Comment, $"%{searchTerm}%"))));
        }
        var startDate = DateTime.SpecifyKind(request.RangeStartDate.Date, DateTimeKind.Unspecified);
        var endDate = DateTime.SpecifyKind(request.RangeEndDate.Date, DateTimeKind.Unspecified).AddDays(1);
        var result = await query
            .Where(x => (x.CreationDatetime >= startDate && x.CreationDatetime <= endDate))
            .Include(x => x.Transaction)
            .SortBy(request.SortBy)
            .Skip(request.ViewCount * request.Page)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken);
        return new GetPurchaseMemberResult(result.Adapt<List<PurchaseDto>>());
    }
}