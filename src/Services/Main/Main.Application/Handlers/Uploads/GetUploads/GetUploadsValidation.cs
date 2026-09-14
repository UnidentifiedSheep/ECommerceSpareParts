using FluentValidation;
using Application.Common;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Uploads.GetUploads;

public class GetUploadsValidation : AbstractValidator<GetUploadsQuery>
{
	public GetUploadsValidation()
	{
		RuleFor(query => query.Cursor.Size)
			.InclusiveBetween(1, 1000)
			.WithLocalizableError(new PaginationSizeRangeMessage().WithStart(1).WithEnd(1000));
	}
}
