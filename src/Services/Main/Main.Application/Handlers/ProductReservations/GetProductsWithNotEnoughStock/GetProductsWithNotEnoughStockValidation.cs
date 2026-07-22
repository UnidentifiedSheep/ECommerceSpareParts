using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;

public class GetProductsWithNotEnoughStockValidation : AbstractValidator<GetProductsWithNotEnoughStockQuery>
{
    public GetProductsWithNotEnoughStockValidation()
    {
        RuleFor(x => x.BuyerOrganizationId)
            .NotEmpty()
            .WithLocalizationKey("article.reservation.organization.id.must.not.be.empty");

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
