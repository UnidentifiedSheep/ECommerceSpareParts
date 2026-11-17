using Main.Application.Document.Models;

namespace Main.Modules.Documents.Application.Interfaces;

public interface IDocumentLinkProvider
{
	Task<DocumentLink> GetOrCreateAsync(
		Guid requestId,
		string bucketName,
		string storageKey,
		DateTime documentExpiresAtUtc);
}
