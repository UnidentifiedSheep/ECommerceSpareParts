using System.Collections.Frozen;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Services.Document;

public sealed class DocumentTemplateResolver(
	IEnumerable<IDocumentProvider> providers,
	IEnumerable<IDocumentTemplateSource> sources)
	: IDocumentTemplateResolver
{
	private readonly FrozenDictionary<DocumentType, IDocumentProvider> _providers =
		providers.ToFrozenDictionary(provider => provider.SupportedType);

	private readonly IDocumentTemplateSource[] _sources = ValidateSources(sources);

	public async Task<IDocumentTemplate?> TryResolveAsync(
		string templateName,
		DocumentType documentType,
		CancellationToken token = default)
	{
		if (!_providers.TryGetValue(documentType, out var provider))
			throw new NotSupportedException($"Document type '{documentType}' is not supported.");

		foreach (var source in _sources)
		{
			var bytes = await source.TryGetBytesAsync(templateName, documentType, token);
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
