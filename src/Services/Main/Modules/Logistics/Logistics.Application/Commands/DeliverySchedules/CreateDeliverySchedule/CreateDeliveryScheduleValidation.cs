using Application.Common.Extensions;
using Domain.Validation;
using FluentValidation;
using Main.Entities.Logistics;
using Main.Entities.Logistics.Schedule;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;

public class CreateDeliveryScheduleValidation : AbstractValidator<CreateDeliveryScheduleCommand>
{
	public CreateDeliveryScheduleValidation()
	{
		RuleFor(x => x.StorageCode)
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(DeliveryScheduleStorageCodeNotEmptyMessage.Instance)
			.Must(x => x.Trim().Length <= DeliverySchedule.StorageCodeMaxLength)
			.WithLocalizableError(DeliveryScheduleStorageCodeMaxLengthMessage.Instance);

		RuleFor(x => x.Days)
			.Must(x => x.IsNotDefault())
			.WithLocalizableError(DeliveryScheduleDaysNotEmptyMessage.Instance)
			.Must(x => x.HasOnlyDefinedFlags())
			.WithLocalizableError(DeliveryScheduleDaysUnsupportedMessage.Instance);
	}
}
