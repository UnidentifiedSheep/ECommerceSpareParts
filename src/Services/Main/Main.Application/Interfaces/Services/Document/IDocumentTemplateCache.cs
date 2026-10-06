using System.Diagnostics.CodeAnalysis;
using Main.Entities.Documents;
using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

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
