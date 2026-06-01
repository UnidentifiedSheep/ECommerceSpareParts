using Abstractions.Models;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Validators;

public class PaginationValidator : AbstractValidator<Pagination>
{
    public PaginationValidator(int? min = null, int? max = null)
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("pagination.page.min");

        RuleFor(query => query.Size)
            .InclusiveBetween(min ?? 1, max ?? 100)
            .WithLocalizationKey("pagination.size.range");
    }
}