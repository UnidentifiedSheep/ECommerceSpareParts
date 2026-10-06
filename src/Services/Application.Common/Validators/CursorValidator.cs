using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Models;
using FluentValidation;

namespace Application.Common.Validators;

public class CursorValidator<T> : AbstractValidator<Cursor<T>>
{
	public CursorValidator()
	{
		RuleFor(query => query.Size)
			.InclusiveBetween(1, 100)
			.WithLocalizableError(PaginationSizeRangeDefaultMessage.Instance);
	}
}
