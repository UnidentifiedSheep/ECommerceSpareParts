using Application.Common.Interfaces.Links;

namespace Main.Application.Models.Options;

public sealed class DocumentLinksOptions
{
	public const string SectionName = "AppLinks";

	public string Document { get; set; } = "documents/{requestId}";

	public bool IsValid() => AppLinkTemplates.IsValid(Document, "requestId");
}
