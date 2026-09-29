using Abstractions.Interfaces;
using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;

namespace Main.Application.Services.Document.Providers;

public sealed class ExcelDocumentProvider(
	IS3StorageService s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions
	) : IDocumentProvider
{
	public DocumentType SupportedType => DocumentType.Excel;

	public async Task<IDocument> GetDocument(
		string templateName,
		string? fallBackTemplateName = null,
		CancellationToken token = default)
	{
		var requestedTemplate = await s3Storage.DownloadFileAsync(
			bucketsOptions.Value.Documents.Name,
			templateName,
			token);
	}
}
