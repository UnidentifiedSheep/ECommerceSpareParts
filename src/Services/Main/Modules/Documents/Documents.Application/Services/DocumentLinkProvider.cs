using Application.Common.Interfaces.Cache;
using Cache.Extensions;
using Main.Application.Document.Models;
using Main.Entities.Documents.Exceptions;
using Main.Modules.Documents.Application.Interfaces;
using S3.Core.Interfaces;

namespace Main.Modules.Documents.Application.Services;

public sealed class DocumentLinkProvider(ICache cache, IS3Service s3Service) : IDocumentLinkProvider
{
	private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(5);

	public async Task<DocumentLink> GetOrCreateAsync(
		Guid requestId,
		string bucketName,
		string storageKey,
		DateTime documentExpiresAtUtc)
	{
		var signedAtUtc = DateTime.UtcNow;
		var lifetime = documentExpiresAtUtc - signedAtUtc;
		if (lifetime <= TimeSpan.Zero)
			throw new DocumentGenerationRequestNotFoundException(requestId);
		if (lifetime > LinkLifetime)
			lifetime = LinkLifetime;

		var cacheTtl = TimeSpan.FromTicks(lifetime.Ticks * 9 / 10);
		var key = DocumentCacheKeys.Link(requestId);
		return await cache.GetOrSetAsync(
			key,
			async () =>
			{
				var url = await s3Service.CreatePresignedDownloadUrl(
					bucketName,
					storageKey,
					lifetime);
				return new DocumentLink(url, signedAtUtc.Add(lifetime));
			},
			cacheTtl) ?? throw new InvalidOperationException("Document link could not be created.");
	}
}
