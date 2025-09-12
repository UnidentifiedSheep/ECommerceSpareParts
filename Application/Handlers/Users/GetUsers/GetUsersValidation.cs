using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Users.GetUsers;

public class GetUsersValidation : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
        RuleFor(query => query.SimilarityLevel)
            .InclusiveBetween(0, 1)
            .WithMessage("Уровень схожести должен быть между 0 и 1");
    }
}