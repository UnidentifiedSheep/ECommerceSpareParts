using Main.Application.Document.Interfaces.Document;
using Main.Application.Document.Models;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Modules.Documents.Application.Services.Document.Sources;

public sealed class EmbeddedDocumentTemplateSource : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => DocumentSourceType.Embedded;

	public async Task<byte[]?> TryGetBytesAsync(
		DocumentTemplateKey key,
		CancellationToken token = default)
	{
		await using var resource = key.Anchor.Assembly.GetManifestResourceStream(key.Anchor, key.FileName);

		if (resource is null) return null;

		using var buffer = new MemoryStream();
		await resource.CopyToAsync(buffer, token);
		return buffer.ToArray();
	}
}
