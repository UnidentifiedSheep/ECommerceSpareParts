using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public class GetStorageOwnersValidation : AbstractValidator<GetStorageOwnersQuery>
{
    public GetStorageOwnersValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}