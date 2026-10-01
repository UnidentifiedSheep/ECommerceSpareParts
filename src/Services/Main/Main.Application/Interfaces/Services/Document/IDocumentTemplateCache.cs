using System.Diagnostics.CodeAnalysis;
using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentTemplateCache
{
	bool TryAddTemplate(
		string key,
		DocumentType type,
		byte[] template);

	bool RemoveTemplate(string key, DocumentType type);

	bool TryGetTemplate(
		string key,
		DocumentType type,
		[NotNullWhen(true)]
		out byte[]? template);
}
