using System.Net;
using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Main.Application.Services.Document.Providers;

public sealed class ExcelDocumentProvider(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions
	) : IDocumentProvider
{
	public DocumentType SupportedType => DocumentType.Excel;

	public async Task<IDocument> GetDocument(
		string templateName,
		string? fallBackTemplateName = null,
		CancellationToken token = default)
	{
		var bucketName = bucketsOptions.Value.Documents.Name;
		var requestedTemplate = await s3Storage.DownloadFileAsync(
			bucketName,
			templateName,
			token);

		var document = await CreateDocumentOrNullAsync(requestedTemplate, templateName, token);
		if (document is not null)
			return document;

		if (fallBackTemplateName is null)
			throw new FileNotFoundException($"Document template '{templateName}' was not found.");

		var fallbackTemplate = await s3Storage.DownloadFileAsync(bucketName, fallBackTemplateName, token);
		return await CreateDocumentOrNullAsync(fallbackTemplate, fallBackTemplateName, token) ??
			throw new FileNotFoundException(
				$"Document templates '{templateName}' and '{fallBackTemplateName}' were not found.");
	}

	private static async Task<IDocument?> CreateDocumentOrNullAsync(
		Response<IStreamResponse> response,
		string templateName,
		CancellationToken token)
	{
		using (response)
		{
			if (response is { IsSuccess: true, Value: not null })
			{
				var templateStream = new MemoryStream();
				try
				{
					await response.Value.Stream.CopyToAsync(templateStream, token);
					templateStream.Position = 0;
					return new ExcelDocument(templateStream);
				}
				catch
				{
					await templateStream.DisposeAsync();
					throw;
				}
			}

			if (response.StatusCode == HttpStatusCode.NotFound)
				return null;

			throw new IOException(
				$"Failed to load document template '{templateName}' from S3: " +
				$"{response.StatusCode} ({response.ErrorCode}).");
		}
	}
}
