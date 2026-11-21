using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Storage.Application.Queries.Storages.GetStorage;

public class GetStoragesValidation : AbstractValidator<GetStoragesQuery>
{
	public GetStoragesValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
