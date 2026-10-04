namespace Main.Application.Dtos.Documents;

public sealed record GetDocumentLinkResult(
	string Url,
	DateTime UrlExpiresAtUtc);
