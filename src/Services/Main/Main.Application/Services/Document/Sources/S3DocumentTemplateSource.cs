using System.Net;
using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Entities.Documents;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Main.Application.Services.Document.Sources;

public class S3DocumentTemplateSource(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions) : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => DocumentSourceType.S3;

	public async Task<byte[]?> TryGetBytesAsync(
		string templateName,
		DocumentType templateType,
		CancellationToken token = default)
	{
		var response = await GetResponseAsync(templateName, token);
		return await ReadTemplateBytesAsync(response, templateName, token);
	}

	protected virtual Task<Response<IStreamResponse>> GetResponseAsync(
		string templateName,
		CancellationToken token)
		=> s3Storage.DownloadFileAsync(
			bucketsOptions.Value.Documents.Name,
			templateName,
			token);

	protected virtual async Task<byte[]?> ReadTemplateBytesAsync(
		Response<IStreamResponse> response,
		string templateName,
		CancellationToken token)
	{
		using (response)
		{
			if (response is { IsSuccess: true, Value: not null })
			{
				using var templateStream = new MemoryStream();
				await response.Value.Stream.CopyToAsync(templateStream, token);
				return templateStream.ToArray();
			}

			if (response.StatusCode == HttpStatusCode.NotFound)
				return null;

			throw new IOException(
				$"Failed to load document template '{templateName}' from S3: " +
				$"{response.StatusCode} ({response.ErrorCode}).");
		}
	}
}
