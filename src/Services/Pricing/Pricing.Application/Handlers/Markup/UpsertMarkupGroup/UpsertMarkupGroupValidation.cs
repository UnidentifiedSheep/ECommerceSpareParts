using FluentValidation;
using Localization.Domain.Extensions;
using Pricing.Application.Dtos.Markup;

namespace Pricing.Application.Handlers.Markup.UpsertMarkupGroup;

public class UpsertMarkupGroupValidation : AbstractValidator<UpsertMarkupGroupCommand>
{
    public UpsertMarkupGroupValidation()
    {
        RuleFor(x => x.MarkupGroup)
            .NotNull()
            .WithLocalizationKey("markup.group.required")
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
            .WithLocalizationKey("markup.group.id.must.be.positive");

        RuleFor(x => x.Name)
            .Must(x => x?.Trim().Length <= 128)
            .WithLocalizationKey("markup.group.name.max.length");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0)
            .WithLocalizationKey("markup.group.currency.id.must.be.positive");

        RuleFor(x => x.Ranges)
            .NotEmpty()
            .WithLocalizationKey("markup.group.ranges.required");

        RuleForEach(x => x.Ranges)
            .SetValidator(new UpsertMarkupRangeDtoValidation());
    }
}

public class UpsertMarkupRangeDtoValidation : AbstractValidator<UpsertMarkupRangeDto>
{
    public UpsertMarkupRangeDtoValidation()
    {
        RuleFor(x => x.RangeStart)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("markup.range.start.must.not.be.negative");

        RuleFor(x => x.RangeEnd)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("markup.range.end.must.not.be.negative");

        RuleFor(x => x.RangeEnd)
            .GreaterThan(x => x.RangeStart)
            .WithLocalizationKey("markup.range.end.must.be.greater.than.start");

        RuleFor(x => x.Markup)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("markup.range.markup.must.not.be.negative");
    }
}
