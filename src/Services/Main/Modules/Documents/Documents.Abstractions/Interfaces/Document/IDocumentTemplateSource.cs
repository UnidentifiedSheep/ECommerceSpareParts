using Enums;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplateSource
{
	DocumentSourceType SourceType { get; }

	Task<byte[]?> TryGetBytesAsync(
		string templateName,
		DocumentType templateType,
		CancellationToken token = default);
}
