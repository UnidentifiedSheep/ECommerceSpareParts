using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Storage.Application.Queries.StorageContents.GetProductStorageContents;

public class GetProductStorageContentsValidation : AbstractValidator<GetProductStorageContentsQuery>
{
	public GetProductStorageContentsValidation()
	{
		RuleForEach(query => query.Items)
			.ChildRules(item => item.RuleFor(x => x.Pagination).SetValidator(new PaginationValidator()));
	}
}
