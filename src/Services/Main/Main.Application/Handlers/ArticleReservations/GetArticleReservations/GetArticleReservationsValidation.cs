using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleReservations.GetArticleReservations;

public class GetArticleReservationsValidation : AbstractValidator<GetArticleReservationsQuery>
{
    public GetArticleReservationsValidation()
    {
        RuleFor(x => x.SearchTerm)
            .MinimumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithLocalizationKey("article.reservation.search.term.min.length");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
        
        RuleFor(query => query.Similarity)
            .InclusiveBetween(0, 1)
            .When(x => x.Similarity != null)
            .WithLocalizationKey("article.reservation.similarity.range");
    }
}