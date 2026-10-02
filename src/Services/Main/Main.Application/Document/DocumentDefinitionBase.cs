using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document;

public abstract class DocumentDefinitionBase<TRequest, TResponse>(
	IDocumentTemplateResolver templateResolver
	) : IDocumentDefinition<TRequest, TResponse>
	where TRequest : IDocumentRequest
	where TResponse : IDocumentResponse
{
	public abstract string SystemName { get; }
	public abstract Type SchemaType { get; }

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

	public abstract Task<TResponse> GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);

	public async Task<IDocumentResponse> GenerateAsync(
		IDocumentRequest data,
		Stream destination,
		CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (data is not TRequest request)
			throw new ArgumentException(
				$"Expected '{typeof(TRequest).FullName}', " +
				$"got '{data.GetType().FullName}'.");

		return await GenerateAsync(request, destination, token);
	}

	protected readonly record struct DocumentTypePair(
		DocumentType OutputDocumentType,
		DocumentType InputTemplateType);
}
