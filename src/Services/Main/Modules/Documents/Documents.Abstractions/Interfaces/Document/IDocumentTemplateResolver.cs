using Enums;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplateResolver
{
	Task<IDocumentTemplate?> TryResolveAsync(
		string templateName,
		DocumentType documentType,
		DocumentType templateType,
		CancellationToken token = default);
}
