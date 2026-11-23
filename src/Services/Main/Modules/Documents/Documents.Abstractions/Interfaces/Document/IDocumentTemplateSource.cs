using Main.Application.Document.Models;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplateSource
{
	DocumentSourceType SourceType { get; }

	Task<byte[]?> TryGetBytesAsync(
		DocumentTemplateKey key,
		CancellationToken token = default);
}
