using Abstractions.Models;
using Application.Common.Extensions;
using FluentValidation;
using Locan.Core.Interfaces;

namespace Application.Common.Validators;

public class PaginationValidator : AbstractValidator<Pagination>
{
	public PaginationValidator(int? min = null, int? max = null)
	{
		var minSize = min ?? 1;
		var maxSize = max ?? 100;
		ILocalizableMessage sizeMessage = min is null && max is null
			? PaginationSizeRangeDefaultMessage.Instance
			: PaginationSizeRangeMessage.Create(minSize, maxSize);

		RuleFor(query => query.Page)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(PaginationPageMinDefaultMessage.Instance);

		RuleFor(query => query.Size)
			.InclusiveBetween(minSize, maxSize)
			.WithLocalizableError(sizeMessage);
	}
}
