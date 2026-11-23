using Enums;
using Main.Application.Document.Models;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplateResolver
{
	Task<IDocumentTemplate?> TryResolveAsync(
		DocumentTemplateKey key,
		DocumentType documentType,
		CancellationToken token = default);
}
