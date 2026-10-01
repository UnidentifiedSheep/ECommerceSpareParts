using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document;

public abstract class DocumentDefinitionBase<TRequest>(IDocumentTemplateResolver templateResolver) :
	IDocumentDefinition<TRequest> where TRequest : IDocumentRequest
{
	public abstract string SystemName { get; }

	protected abstract string BaseTemplateKey { get; }
	protected abstract DocumentTypePair[] SupportedTypePairs { get; }

	protected virtual DocumentType GetTemplateType(TRequest request)
	{
		foreach (var (outputDocumentType, inputTemplateType) in SupportedTypePairs)
			if (outputDocumentType == request.DocumentType) return inputTemplateType;

		throw new NotSupportedException(
			$"Document type '{request.DocumentType}' is not supported for '{SystemName}'.");
	}

	protected async Task<IDocumentTemplate> GetTemplateAsync(
		TRequest request,
		CancellationToken token = default)
	{
		var templateType = GetTemplateType(request);
		var fullKey = GetFullTemplateKey(templateType);
		return await templateResolver.TryResolveAsync(fullKey, request.DocumentType, templateType, token)
			?? throw new InvalidOperationException(
				$"Unable to find template for '{SystemName}' and key '{fullKey}'");
	}

	private string GetFullTemplateKey(DocumentType templateType)
		=> BaseTemplateKey + templateType.GetFileExtension();

	public abstract Task GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);

	protected readonly record struct DocumentTypePair(
		DocumentType OutputDocumentType,
		DocumentType InputTemplateType);
}
