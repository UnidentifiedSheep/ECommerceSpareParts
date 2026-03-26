using Abstractions.Models;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Validators;

public class PaginationValidator : AbstractValidator<PaginationModel>
{
    public PaginationValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("pagination.page.min");

        RuleFor(query => query.Size)
            .InclusiveBetween(1, 100)
            .WithLocalizationKey("pagination.size.range");
    }
}