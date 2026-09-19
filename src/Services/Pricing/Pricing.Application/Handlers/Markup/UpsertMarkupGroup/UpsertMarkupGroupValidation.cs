using Application.Common.Extensions;
using FluentValidation;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markup.UpsertMarkupGroup;

public class UpsertMarkupGroupValidation : AbstractValidator<UpsertMarkupGroupCommand>
{
	public UpsertMarkupGroupValidation()
	{
		RuleFor(x => x.MarkupGroup)
			.NotNull()
			.WithLocalizableError(MarkupGroupRequiredMessage.Instance)
			.SetValidator(new UpsertMarkupGroupDtoValidation());
	}
}

public class UpsertMarkupGroupDtoValidation : AbstractValidator<UpsertMarkupGroupDto>
{
	public UpsertMarkupGroupDtoValidation()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0)
			.When(x => x.Id.HasValue)
			.WithLocalizableError(MarkupGroupIdMustBePositiveMessage.Instance);

		RuleFor(x => x.Name)
			.Must(x => x?.Trim().Length <= 128)
			.WithLocalizableError(MarkupGroupNameMaxLengthMessage.Instance);

		RuleFor(x => x.CurrencyId)
			.GreaterThan(0)
			.WithLocalizableError(MarkupGroupCurrencyIdMustBePositiveMessage.Instance);

		RuleFor(x => x.Ranges).NotEmpty().WithLocalizableError(MarkupGroupRangesRequiredMessage.Instance);

		RuleForEach(x => x.Ranges).SetValidator(new UpsertMarkupRangeDtoValidation());
	}
}

public class UpsertMarkupRangeDtoValidation : AbstractValidator<UpsertMarkupRangeDto>
{
	public UpsertMarkupRangeDtoValidation()
	{
		RuleFor(x => x.RangeStart)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(MarkupRangeStartMustNotBeNegativeMessage.Instance);

		RuleFor(x => x.RangeEnd)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(MarkupRangeEndMustNotBeNegativeMessage.Instance);

		RuleFor(x => x.RangeEnd)
			.GreaterThanOrEqualTo(x => x.RangeStart)
			.WithLocalizableError(MarkupRangeEndMustNotBeLessThanStartMessage.Instance);

		RuleFor(x => x.Markup)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(MarkupRangeMarkupMustNotBeNegativeMessage.Instance);
	}
}
