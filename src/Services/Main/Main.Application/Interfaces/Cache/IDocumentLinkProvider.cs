using Main.Application.Dtos.Documents;

namespace Main.Application.Interfaces.Cache;

public interface IDocumentLinkProvider
{
	Task<GetDocumentLinkResult> GetOrCreateAsync(
		Guid requestId,
		string bucketName,
		string storageKey,
		DateTime documentExpiresAtUtc);
}
