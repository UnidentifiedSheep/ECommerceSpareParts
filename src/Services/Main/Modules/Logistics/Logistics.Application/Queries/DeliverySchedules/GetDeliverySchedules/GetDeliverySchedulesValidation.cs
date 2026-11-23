using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Logistics.Application.Queries.DeliverySchedules.GetDeliverySchedules;

public class GetDeliverySchedulesValidation : AbstractValidator<GetDeliverySchedulesQuery>
{
	public GetDeliverySchedulesValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
