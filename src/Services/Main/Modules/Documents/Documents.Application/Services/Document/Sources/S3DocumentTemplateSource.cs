using System.Net;
using Main.Application.Document.Interfaces.Document;
using Main.Application.Document.Models;
using Main.Entities.Documents.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Models;
using S3.Core.Options;

namespace Main.Modules.Documents.Application.Services.Document.Sources;

public class S3DocumentTemplateSource(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions) : IDocumentTemplateSource
{
	public DocumentSourceType SourceType => DocumentSourceType.S3;

	public async Task<byte[]?> TryGetBytesAsync(
		DocumentTemplateKey key,
		CancellationToken token = default)
	{
		var response = await GetResponseAsync(key.ObjectKey, token);
		return await ReadTemplateBytesAsync(
			response,
			key.ObjectKey,
			token);
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
