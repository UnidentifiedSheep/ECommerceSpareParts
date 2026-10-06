using Main.Entities.Documents;
using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentTemplateSource
{
	DocumentSourceType SourceType { get; }

	Task<byte[]?> TryGetBytesAsync(
		string templateName,
		DocumentType templateType,
		CancellationToken token = default);
}
