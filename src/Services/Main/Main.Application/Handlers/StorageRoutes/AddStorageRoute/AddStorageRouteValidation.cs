using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.AddStorageRoute;

public class AddStorageRouteValidation : AbstractValidator<AddStorageRouteCommand>
{
	public AddStorageRouteValidation()
	{
		RuleFor(x => new
			{
				x.StorageTo, x.StorageFrom
			})
			.Must(x => x.StorageTo != x.StorageFrom)
			.WithLocalizableError(StorageRouteSameStoragesMessage.Instance);

		RuleFor(x => x.Distance)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(StorageRouteDistanceMinMessage.Instance);

		RuleFor(x => x.DeliveryTime)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(StorageRouteDeliveryTimeMinMessage.Instance);

		RuleFor(x => x.PriceKg)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceKgMinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceKgPrecisionMessage.Instance);

		RuleFor(x => x.PriceM3)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceM3MinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceM3PrecisionMessage.Instance);

		RuleFor(x => x.PricePerOrder)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(StorageRoutePriceOrderMinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(StorageRoutePriceOrderPrecisionMessage.Instance);
	}
}
