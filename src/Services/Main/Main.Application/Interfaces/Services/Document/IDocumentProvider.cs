using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentProvider
{
	DocumentType SupportedType { get; }

	Task<IDocument> GetDocument(
		string templateName,
		string? fallBackTemplateName = null,
		CancellationToken token = default);
}
