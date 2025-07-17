using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.GetArticleReservations;

public record GetArticleReservationsQuery(string? SearchTerm, int Page, int ViewCount, string? SortBy, string? UserId) : IQuery<GetArticleReservationsResult>;
public record GetArticleReservationsResult(IEnumerable<ArticleReservationDto> Reservations);

public class GetArticleReservationsValidation : AbstractValidator<GetArticleReservationsQuery>
{
    public GetArticleReservationsValidation()
    {
        RuleFor(x => x.SearchTerm)
            .MinimumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Минимальная длинна строки поиска 3");
        
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}
    
public class GetArticleReservationsHandler(DContext context) : IQueryHandler<GetArticleReservationsQuery, GetArticleReservationsResult>
{
    public async Task<GetArticleReservationsResult> Handle(GetArticleReservationsQuery request, CancellationToken cancellationToken)
    {
        var query = context.StorageContentReservations
            .AsNoTracking();
        if(request.UserId != null)
            query = query.Where(x => x.UserId == request.UserId);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var normalizedSearchTerm = request.SearchTerm.ToNormalizedArticleNumber();
            query = query.Include(x => x.Article)
                .Where(x => (x.Comment == null || EF.Functions.ILike(x.Comment, $"%{request.SearchTerm}%")) ||
                            EF.Functions.ILike(x.Article.NormalizedArticleNumber, $"%{normalizedSearchTerm}%") ||
                             EF.Functions.ToTsVector("russian", x.Article.ArticleName)
                                 .Matches(EF.Functions.PlainToTsQuery("russian", request.SearchTerm)));
        }
        query = query.SortBy(request.SortBy)
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount);
        
        var result = (await query.ToListAsync(cancellationToken)).Adapt<List<ArticleReservationDto>>();
        return new GetArticleReservationsResult(result);
    }
}