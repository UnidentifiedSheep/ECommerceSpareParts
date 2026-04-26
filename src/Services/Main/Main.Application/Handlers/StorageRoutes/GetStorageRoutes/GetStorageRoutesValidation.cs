using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public class GetStorageRoutesValidation : AbstractValidator<GetStorageRoutesQuery>
{
    public GetStorageRoutesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}