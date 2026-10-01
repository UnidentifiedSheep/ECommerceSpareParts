using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentTemplateResolver
{
	Task<IDocumentTemplate?> TryResolveAsync(
		string templateName,
		DocumentType documentType,
		CancellationToken token = default);
}
