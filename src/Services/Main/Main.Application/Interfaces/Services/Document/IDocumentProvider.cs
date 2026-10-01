using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentProvider
{
	DocumentType SupportedType { get; }

	Task<IDocumentTemplate?> TryGetDocumentTemplate(
		string templateName,
		CancellationToken token = default);
}
