using Main.Application.Document.Interfaces.Document;
using Main.Application.Document.Models;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Modules.Documents.Application.Services.Document.Sources;

public sealed class CachedDocumentTemplateSource(
	IDocumentTemplateSource source,
	IDocumentTemplateCache cache) : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => source.SourceType;

	public async Task<byte[]?> TryGetBytesAsync(
		DocumentTemplateKey key,
		CancellationToken token = default)
	{
		if (cache.TryGetTemplate(
				key.ObjectKey,
				key.TemplateType,
				SourceType,
				out var bytes))
			return bytes;

		bytes = await source.TryGetBytesAsync(key, token);
		if (bytes is not null)
			cache.TryAddTemplate(
				key.ObjectKey,
				key.TemplateType,
				SourceType,
				bytes);
		return bytes;
	}
}
