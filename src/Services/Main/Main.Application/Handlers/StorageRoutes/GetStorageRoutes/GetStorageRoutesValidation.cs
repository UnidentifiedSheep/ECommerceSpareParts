using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public class GetStorageRoutesValidation : AbstractValidator<GetStorageRoutesQuery>
{
    public GetStorageRoutesValidation()
    {
        RuleFor(x => x.PaginationModel)
            .SetValidator(new PaginationValidator());
    }
}