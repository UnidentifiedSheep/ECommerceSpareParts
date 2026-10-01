using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document;

public abstract class DocumentDefinitionBase<TRequest>(
	IDocumentTemplateResolver templateResolver,
	(DocumentType OutputDocumentType, DocumentType InputTemplateType)[] supportedTypes) :
	IDocumentDefinition<TRequest> where TRequest : IDocumentRequest
{
	private readonly (DocumentType OutputDocumentType, DocumentType InputTemplateType)[] _supportedTypes =
		ValidateSupportedTypes(supportedTypes);

	public abstract string SystemName { get; }

	protected abstract string BaseTemplateKey { get; }

	protected virtual DocumentType GetTemplateType(TRequest request)
	{
		foreach (var (outputDocumentType, inputTemplateType) in _supportedTypes)
			if (outputDocumentType == request.DocumentType) return inputTemplateType;

		throw new NotSupportedException(
			$"Document type '{request.DocumentType}' is not supported for '{SystemName}'.");
	}

	protected Task<IDocumentTemplate?> TryGetTemplateAsync(
		TRequest request,
		CancellationToken token = default)
	{
		var templateType = GetTemplateType(request);
		var fullKey = GetFullTemplateKey(templateType);
		return templateResolver.TryResolveAsync(fullKey, request.DocumentType, templateType, token);
	}

	private string GetFullTemplateKey(DocumentType templateType)
		=> BaseTemplateKey + templateType.GetFileExtension();

	private static (DocumentType OutputDocumentType, DocumentType InputTemplateType)[] ValidateSupportedTypes(
		(DocumentType OutputDocumentType, DocumentType InputTemplateType)[] supportedTypes)
	{
		ArgumentNullException.ThrowIfNull(supportedTypes);
		if (supportedTypes.Length == 0)
			throw new ArgumentException("At least one document type must be supported.", nameof(supportedTypes));

		var outputTypes = new HashSet<DocumentType>();
		foreach (var (outputDocumentType, _) in supportedTypes)
			if (!outputTypes.Add(outputDocumentType))
				throw new ArgumentException(
					$"Output document type '{outputDocumentType}' is registered more than once.",
					nameof(supportedTypes));

		return supportedTypes.ToArray();
	}

	public abstract Task GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);
}
