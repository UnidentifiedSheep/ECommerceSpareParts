using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Documents.SearchDocuments;

public class SearchDocumentsValidation : AbstractValidator<SearchDocumentsQuery>
{
	public SearchDocumentsValidation()
	{
		RuleFor(x => x.Pagination)
			.NotNull()
			.WithLocalizableError(DocumentSearchPaginationRequiredMessage.Instance)
			.SetValidator(new PaginationValidator());

		RuleFor(x => x.CallerId)
			.Must(id => id != null && id.Value != Guid.Empty)
			.WithLocalizableError(DocumentSearchCallerIdRequiredMessage.Instance)
			.When(x => !x.CanAccessAll);

		RuleFor(x => x.DocumentSystemName)
			.MaximumLength(128)
			.WithLocalizableError(DocumentSearchSystemNameTooLongMessage.Instance);
	}
}
