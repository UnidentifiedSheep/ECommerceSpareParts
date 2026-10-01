namespace Main.Enums.Documents;

public static class DocumentTypeExtensions
{
	public static string GetFileExtension(this DocumentType type) => type switch
	{
		DocumentType.Pdf => ".pdf",
		DocumentType.Excel => ".xlsx",
		DocumentType.Html => ".html",
		DocumentType.Docx => ".docx",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
}
