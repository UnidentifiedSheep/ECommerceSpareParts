using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Users.GetUsers;

public class GetUsersValidation : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());

        RuleFor(query => query.SimilarityLevel)
            .InclusiveBetween(0, 1)
            .WithLocalizationKey("user.similarity.level.range");
    }
}