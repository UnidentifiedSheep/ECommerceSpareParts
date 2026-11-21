using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Storage.Application.Queries.StorageContents.GetStorageContents;

public sealed class GetStorageContentsValidation : AbstractValidator<GetStorageContentsQuery>
{
	public GetStorageContentsValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
