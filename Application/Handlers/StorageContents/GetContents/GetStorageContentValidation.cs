using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.StorageContents.GetContents;

public class GetStorageContentValidation : AbstractValidator<GetStorageContentQuery>
{
    public GetStorageContentValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
    }
}