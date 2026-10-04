using Application.Common.Interfaces.Cache;
using Cache.Extensions;
using Main.Application.Dtos.Documents;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Entities.Exceptions;
using S3.Core.Interfaces;

namespace Main.Cache;

public sealed class DocumentLinkProvider(ICache cache, IS3Service s3Service) : IDocumentLinkProvider
{
	private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(5);

	public async Task<GetDocumentLinkResult> GetOrCreateAsync(
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
		var key = CacheKeys.DocumentCache.Link(requestId);
		return await cache.GetOrSetAsync(
			key,
			async () =>
			{
				var url = await s3Service.CreatePresignedDownloadUrl(bucketName, storageKey, lifetime);
				return new GetDocumentLinkResult(url, signedAtUtc.Add(lifetime));
			},
			cacheTtl) ?? throw new InvalidOperationException("Document link could not be created.");
	}
}
