using Abstractions.Models;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Validators;

public class CursorValidator<T> : AbstractValidator<Cursor<T>>
{
    public CursorValidator()
    {
        RuleFor(query => query.Size)
            .InclusiveBetween(1, 100)
            .WithLocalizationKey("pagination.size.range");
    }
}