using System.Diagnostics.CodeAnalysis;
using Enums;
using Main.Entities.Documents.Enums.Documents;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplateCache
{
	bool TryAddTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType,
		byte[] template);

	bool RemoveTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType);

	bool TryGetTemplate(
		string key,
		DocumentType type,
		DocumentSourceType sourceType,
		[NotNullWhen(true)] out byte[]? template);
}
