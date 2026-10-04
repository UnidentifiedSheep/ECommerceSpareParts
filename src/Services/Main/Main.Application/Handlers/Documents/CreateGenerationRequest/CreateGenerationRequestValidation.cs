using Application.Common.Extensions;
using Domain.Validation;
using Extensions;
using FluentValidation;
using Main.Application.Interfaces.Services.Document;
using Main.Entities;
using NamedObject.Core.Interfaces;

namespace Main.Application.Handlers.Documents.CreateGenerationRequest;

public class CreateGenerationRequestValidation : AbstractValidator<CreateGenerationRequestCommand>
{
	public CreateGenerationRequestValidation(INamedObjectRegistry<IDocumentDefinition> registry)
	{
		RuleFor(x => x.DocumentSystemName)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(LrtDocumentGenerationSystemNameRequiredMessage.Instance)
			.MaximumLength(128)
			.WithLocalizableError(LrtDocumentGenerationSystemNameTooLongMessage.Instance)
			.Must(x => registry.TryGetBySystemName(x) is not null)
			.WithLocalizableError(LrtDocumentGenerationUnknownSystemNameMessage.Instance);

		RuleFor(x => x.DocumentRequest)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(LrtDocumentGenerationRequestRequiredMessage.Instance)
			.Must((command, json) => IsValidRequest(command, json, registry))
			.WithLocalizableError(LrtDocumentGenerationInvalidRequestMessage.Instance);

		RuleFor(x => x.RequesterId)
			.Must(id => id is null || id != Guid.Empty)
			.WithLocalizableError(LrtDocumentGenerationInvalidRequesterIdMessage.Instance);
	}

	private static bool IsValidRequest(
		CreateGenerationRequestCommand command,
		string json,
		INamedObjectRegistry<IDocumentDefinition> registry)
	{
		var definition = string.IsNullOrWhiteSpace(command.DocumentSystemName)
			? null
			: registry.TryGetBySystemName(command.DocumentSystemName);

		if (definition is null) return json.IsValidJson();

		return json.TryDeserializeJson(definition.RequestType, out var value) &&
		       value is IDocumentRequest request &&
		       definition.SupportsDocumentType(request.DocumentType);
	}
}
