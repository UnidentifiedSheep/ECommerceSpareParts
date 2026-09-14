using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

public class EditStorageRouteValidation : AbstractValidator<EditStorageRouteCommand>
{
	public EditStorageRouteValidation()
	{
		RuleFor(x => x.PatchStorageRoute.PriceKg.Value)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceKgMinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceKgPrecisionMessage.Instance)
			.When(x => x.PatchStorageRoute.PriceKg.IsSet);

		RuleFor(x => x.PatchStorageRoute.PricePerM3.Value)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceM3MinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceM3PrecisionMessage.Instance)
			.When(x => x.PatchStorageRoute.PricePerM3.IsSet);

		RuleFor(x => x.PatchStorageRoute.PricePerOrder.Value)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceOrderMinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceOrderPrecisionMessage.Instance)
			.When(x => x.PatchStorageRoute.PricePerOrder.IsSet);

		RuleFor(x => x.PatchStorageRoute.DistanceM.Value)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(StorageRouteDistanceMinMessage.Instance)
			.When(x => x.PatchStorageRoute.DistanceM.IsSet);

		RuleFor(x => x.PatchStorageRoute.DeliveryTimeMinutes.Value)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(StorageRouteDeliveryTimeMinMessage.Instance)
			.When(x => x.PatchStorageRoute.DeliveryTimeMinutes.IsSet);
	}
}
