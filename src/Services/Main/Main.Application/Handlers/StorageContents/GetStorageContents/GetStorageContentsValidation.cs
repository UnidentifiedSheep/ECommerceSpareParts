using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.StorageContents.GetStorageContents;

public sealed class GetStorageContentsValidation : AbstractValidator<GetStorageContentsQuery>
{
	public GetStorageContentsValidation()
	{
		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
