using Main.Application.Models.Documents;

namespace Main.Application.Interfaces.Cache;

public interface IDocumentLinkProvider
{
	Task<DocumentLink> GetOrCreateAsync(
		Guid requestId,
		string bucketName,
		string storageKey,
		DateTime documentExpiresAtUtc);
}
