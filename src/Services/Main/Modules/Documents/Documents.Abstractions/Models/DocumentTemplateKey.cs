using Enums;

namespace Main.Application.Document.Models;

public sealed record DocumentTemplateKey(
	Type Anchor,
	string Group,
	string SystemName,
	string? CultureName,
	DocumentType TemplateType)
{
	public string FileName
		=> CultureName is null
			? $"{SystemName}{TemplateType.GetFileExtension()}"
			: $"{SystemName}.{CultureName}{TemplateType.GetFileExtension()}";

	public string ObjectKey => $"{Group}/{FileName}";
}
