using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Uploads.GetUploads;

public class GetUploadsValidation : AbstractValidator<GetUploadsQuery>
{
    public GetUploadsValidation()
    {
        RuleFor(query => query.Cursor.Size)
            .InclusiveBetween(1, 1000)
            .WithLocalizationKey("pagination.size.range");
    }
}