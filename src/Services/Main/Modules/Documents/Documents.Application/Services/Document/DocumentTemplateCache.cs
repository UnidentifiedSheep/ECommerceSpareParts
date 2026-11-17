using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Enums;
using Main.Application.Document.Interfaces.Document;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Modules.Documents.Application.Services.Document;

public sealed class DocumentTemplateCache : IDocumentTemplateCache
{
	private readonly ConcurrentDictionary<Key, byte[]> _cache = new();

	public bool TryAddTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType,
		byte[] template)
		=> _cache.TryAdd(
			new Key(
				key,
				type,
				sourceType),
			template);

	public bool RemoveTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType)
		=> _cache.TryRemove(
			new Key(
				key,
				type,
				sourceType),
			out _);

	public bool TryGetTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType,
		[NotNullWhen(true)] out byte[]? template)
		=> _cache.TryGetValue(
			new Key(
				key,
				type,
				sourceType),
			out template);

	private readonly record struct Key(
		string TemplateName,
		DocumentType Type,
		DocumentSourceType SourceType);
}
