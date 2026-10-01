using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Services.Document;

public sealed class DocumentTemplateCache : IDocumentTemplateCache
{
	private readonly ConcurrentDictionary<Key, byte[]> _cache = new();

	public bool TryAddTemplate(
		string key,
		DocumentType type,
		byte[] template)
		=> _cache.TryAdd(new Key(key, type), template);

	public bool RemoveTemplate(string key, DocumentType type)
		=> _cache.TryRemove(new Key(key, type), out _);

	public bool TryGetTemplate(
		string key,
		DocumentType type,
		[NotNullWhen(true)]
		out byte[]? template)
		=> _cache.TryGetValue(new Key(key, type), out template);

	private readonly record struct Key(string TemplateName, DocumentType Type);
}
