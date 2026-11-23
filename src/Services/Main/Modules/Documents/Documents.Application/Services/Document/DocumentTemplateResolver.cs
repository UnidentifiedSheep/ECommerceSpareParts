using System.Collections.Frozen;
using Enums;
using Main.Application.Document.Interfaces.Document;
using Main.Application.Document.Models;

namespace Main.Modules.Documents.Application.Services.Document;

public sealed class DocumentTemplateResolver(
	IEnumerable<IDocumentProvider> providers,
	IEnumerable<IDocumentTemplateSource> sources)
	: IDocumentTemplateResolver
{
	private readonly FrozenDictionary<(DocumentType Output, DocumentType Input), IDocumentProvider>
		_providers =
			providers.ToFrozenDictionary(provider
				=> (provider.OutputDocumentType, provider.InputTemplateType));

	private readonly IDocumentTemplateSource[] _sources = ValidateSources(sources);

	public async Task<IDocumentTemplate?> TryResolveAsync(
		DocumentTemplateKey key,
		DocumentType documentType,
		CancellationToken token = default)
	{
		if (!_providers.TryGetValue((documentType, key.TemplateType), out var provider))
			throw new NotSupportedException(
				$"Document type '{documentType}' with template type '{key.TemplateType}' is not supported.");

		foreach (var source in _sources)
		{
			var bytes = await source.TryGetBytesAsync(key, token);
			if (bytes is not null)
				return provider.CreateDocument(bytes);
		}

		return null;
	}

	private static IDocumentTemplateSource[] ValidateSources(
		IEnumerable<IDocumentTemplateSource> sources)
	{
		var registered = sources.ToArray();
		return registered
			.GroupBy(source => source.SourceType)
			.Any(group => group.Count() > 1)
			? throw new InvalidOperationException(
				"Document template source types must be unique.")
			: registered;
	}
}
