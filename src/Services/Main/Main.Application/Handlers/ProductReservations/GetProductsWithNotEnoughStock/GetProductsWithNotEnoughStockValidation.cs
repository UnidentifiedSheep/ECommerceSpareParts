using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;

public class GetProductsWithNotEnoughStockValidation : AbstractValidator<GetProductsWithNotEnoughStockQuery>
{
	public GetProductsWithNotEnoughStockValidation()
	{
		RuleFor(x => x.BuyerOrganizationId)
			.NotEmpty()
			.WithLocalizableError(ArticleReservationOrganizationIdMustNotBeEmptyMessage.Instance);

		RuleFor(x => x.StorageCode)
			.NotEmpty()
			.WithLocalizableError(ArticleReservationStorageNameMustNotBeEmptyMessage.Instance);

		RuleForEach(x => x.NeededCounts)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Value)
					.GreaterThan(0)
					.WithLocalizableError(ArticleReservationNeededCountMustBePositiveMessage.Instance);
			});
	}
}
