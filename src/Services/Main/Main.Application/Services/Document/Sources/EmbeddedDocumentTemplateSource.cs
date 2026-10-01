using Main.Application.Interfaces.Services.Document;
using Main.Entities.Documents;
using Main.Enums.Documents;

namespace Main.Application.Services.Document.Sources;

public sealed class EmbeddedDocumentTemplateSource : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => DocumentSourceType.Embedded;

	public async Task<byte[]?> TryGetBytesAsync(
		string templateName,
		DocumentType templateType,
		CancellationToken token = default)
	{
		var assembly = typeof(EmbeddedDocumentTemplateSource).Assembly;
		var resourceName = $"{assembly.GetName().Name}.Document.{templateName.Replace('/', '.')}";
		await using var resource = assembly.GetManifestResourceStream(resourceName);

		if (resource is null) return null;

		using var buffer = new MemoryStream();
		await resource.CopyToAsync(buffer, token);
		return buffer.ToArray();
	}
}
