using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentTemplateResolver
{
	Task<IDocumentTemplate?> TryResolveAsync(
		string templateName,
		DocumentType documentType,
		DocumentType templateType,
		CancellationToken token = default);
}
