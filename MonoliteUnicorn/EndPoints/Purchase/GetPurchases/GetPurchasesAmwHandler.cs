using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Purchase.GetPurchases;

public record GetPurchasesAmwQuery(DateTime RangeStartDate, DateTime RangeEndDate, int Page, int ViewCount, string? SupplierId, 
    int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetPurchasesAmwResult>;
public record GetPurchasesAmwResult(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesAmwValidation : AbstractValidator<GetPurchasesAmwQuery>
{
    public GetPurchasesAmwValidation()
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

public class GetPurchasesAmwHandler(DContext context) : IQueryHandler<GetPurchasesAmwQuery, GetPurchasesAmwResult>
{
    public async Task<GetPurchasesAmwResult> Handle(GetPurchasesAmwQuery request, CancellationToken cancellationToken)
    {
        var query = context.Purchases.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.SupplierId))
        {
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SupplierId && x.IsSupplier == true, cancellationToken) ?? throw new SupplierNotFoundException(request.SupplierId);
            query = query.Where(x => x.SupplierId == request.SupplierId);
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
            query = query.Where(x => x.PurchaseContents
                .Any(content => (EF.Functions.ToTsVector("russian", content.Article.ArticleName)
                                     .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                 EF.Functions.ILike(content.Article.NormalizedArticleNumber, $"%{normalizedSearchTerm}%"))) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")));
        }
        var startDate = DateTime.SpecifyKind(request.RangeStartDate.Date, DateTimeKind.Unspecified);
        var endDate = DateTime.SpecifyKind(request.RangeEndDate.Date, DateTimeKind.Unspecified).AddDays(1);
        var result = await query.Where(x => (x.CreationDatetime >= startDate && x.CreationDatetime <= endDate))
            .Include(x => x.Transaction)
            .Include(x => x.Supplier)
            .SortBy(request.SortBy)
            .Skip(request.ViewCount * request.Page)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken);
        return new GetPurchasesAmwResult(result.Adapt<List<PurchaseDto>>());
    }
}