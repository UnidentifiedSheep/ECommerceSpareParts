using Main.Application.Interfaces.Services.Document;
using Main.Entities.Documents;
using Main.Enums.Documents;

namespace Main.Application.Services.Document.Sources;

public sealed class CachedDocumentTemplateSource(
	IDocumentTemplateSource source,
	IDocumentTemplateCache cache) : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => source.SourceType;

	public async Task<byte[]?> TryGetBytesAsync(
		string templateName,
		DocumentType templateType,
		CancellationToken token = default)
	{
		if (cache.TryGetTemplate(templateName, templateType, SourceType, out var bytes))
			return bytes;

		bytes = await source.TryGetBytesAsync(templateName, templateType, token);
		if (bytes is not null)
			cache.TryAddTemplate(templateName, templateType, SourceType, bytes);
		return bytes;
	}
}
