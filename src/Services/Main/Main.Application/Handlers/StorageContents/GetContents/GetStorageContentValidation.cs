using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.GetContents;

public class GetStorageContentValidation : AbstractValidator<GetStorageContentQuery>
{
    public GetStorageContentValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
    }
}