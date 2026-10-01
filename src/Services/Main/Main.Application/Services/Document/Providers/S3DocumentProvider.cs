using System.Net;
using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Main.Application.Services.Document.Providers;

public abstract class S3DocumentProvider<TDocument>(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions
	) : IDocumentProvider
	where TDocument : IDocumentTemplate
{
	public abstract DocumentType SupportedType { get; }

	public virtual async Task<IDocumentTemplate?> TryGetDocumentTemplate(
		string templateName,
		CancellationToken token = default)
	{
		var bytes = await GetTemplateBytesAsync(templateName, token);
		return bytes is null ? null : CreateDocument(bytes);
	}

	protected virtual Task<Response<IStreamResponse>> GetResponseAsync(
		string templateName,
		CancellationToken token)
		=> s3Storage.DownloadFileAsync(
			bucketsOptions.Value.Documents.Name,
			templateName,
			token);

	protected async Task<byte[]?> GetTemplateBytesAsync(
		string templateName,
		CancellationToken token)
	{
		var response = await GetResponseAsync(templateName, token);
		return await ReadTemplateBytesAsync(response, templateName, token);
	}

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

	protected TDocument CreateDocument(byte[] bytes)
	{
		var stream = new MemoryStream(bytes, writable: false);
		try
		{
			return GenDocument(stream);
		}
		catch
		{
			stream.Dispose();
			throw;
		}
	}

	protected abstract TDocument GenDocument(MemoryStream stream);
}
