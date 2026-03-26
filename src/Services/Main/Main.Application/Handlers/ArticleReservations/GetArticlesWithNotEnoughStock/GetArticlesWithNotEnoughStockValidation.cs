using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;

public class GetArticlesWithNotEnoughStockValidation : AbstractValidator<GetArticlesWithNotEnoughStockQuery>
{
    public GetArticlesWithNotEnoughStockValidation()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty()
            .WithLocalizationKey("article.reservation.buyer.id.must.not.be.empty");

        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithLocalizationKey("article.reservation.storage.name.must.not.be.empty");

        RuleForEach(x => x.NeededCounts)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .GreaterThan(0)
                    .WithLocalizationKey("article.reservation.needed.count.must.be.positive");
            });
    }
}