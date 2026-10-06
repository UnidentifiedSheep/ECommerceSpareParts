using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Documents.SearchDocuments;

public class SearchDocumentsValidation : AbstractValidator<SearchDocumentsQuery>
{
	public SearchDocumentsValidation()
	{
		RuleFor(x => x.Pagination)
			.NotNull()
			.SetValidator(new PaginationValidator());

		RuleFor(x => x.CallerId)
			.Must(id => id != null && id.Value != Guid.Empty)
			.When(x => !x.CanAccessAll);

		RuleFor(x => x.DocumentSystemName)
			.MaximumLength(128);
	}
}
