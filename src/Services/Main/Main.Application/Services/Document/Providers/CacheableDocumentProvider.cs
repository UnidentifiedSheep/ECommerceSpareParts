using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Services.Document.Providers;

public abstract class CacheableDocumentProvider<TDocument>(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions,
	IDocumentTemplateCache cache
	) : S3DocumentProvider<TDocument>(s3Storage, bucketsOptions)
	where TDocument : IDocumentTemplate
{
	public override async Task<IDocumentTemplate?> TryGetDocumentTemplate(
		string templateName,
		CancellationToken token = default)
	{
		if (cache.TryGetTemplate(templateName, SupportedType, out var bytes))
			return CreateDocument(bytes);

		bytes = await GetTemplateBytesAsync(templateName, token);
		if (bytes is null) return null;

		var document = CreateDocument(bytes);
		cache.TryAddTemplate(templateName, SupportedType, bytes);
		return document;
	}
}
