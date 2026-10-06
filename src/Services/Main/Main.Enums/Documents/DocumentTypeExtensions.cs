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

	public static string GetContentType(this DocumentType type) => type switch
	{
		DocumentType.Pdf => "application/pdf",
		DocumentType.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
		DocumentType.Html => "text/html",
		DocumentType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
}
