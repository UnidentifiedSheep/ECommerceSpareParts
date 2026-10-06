using Domain.CommonEnums;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Documents;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.Document;

[GraphQLName("DocumentGenerationRequest")]
public record GqlDocumentGenerationRequest
{
	private readonly DocumentGenerationRequestDto? _document;

	public GqlDocumentGenerationRequest(Guid requestId)
	{
		RequestId = requestId;
	}

	public GqlDocumentGenerationRequest(
		DocumentGenerationRequestDto document) : this(document.RequestId)
	{
		_document = document;
	}

	[GraphQLName("requestId")]
	public Guid RequestId { get; }

	[GraphQLName("jobId")]
	public async Task<Guid> GetJobIdAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).JobId;

	[GraphQLName("status")]
	public async Task<JobStatus> GetStatusAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).Status;

	[GraphQLName("documentSystemName")]
	public async Task<string> GetDocumentSystemNameAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).DocumentSystemName;

	[GraphQLName("requesterId")]
	public async Task<Guid?> GetRequesterIdAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).RequesterId;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).CreatedAt;

	[GraphQLName("generatedAt")]
	public async Task<DateTime?> GetGeneratedAtAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).GeneratedAt;

	[GraphQLName("expiresAt")]
	public async Task<DateTime?> GetExpiresAtAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetDocumentAsync(loader, cancellationToken)).ExpiresAt;

	[GraphQLName("fileLink")]
	public async Task<GqlDocumentLink?> GetFileLinkAsync(
		IDocumentGenerationRequestsDataLoader loader,
		IDocumentLinkProvider linkProvider,
		CancellationToken cancellationToken)
	{
		var document = await GetDocumentAsync(loader, cancellationToken);
		if (document.BucketName is null || document.StorageKey is null || document.ExpiresAt is null)
			return null;

		var link = await linkProvider.GetOrCreateAsync(
			RequestId,
			document.BucketName,
			document.StorageKey,
			document.ExpiresAt.Value);

		return new GqlDocumentLink(link.Url, link.UrlExpiresAtUtc);
	}

	private async Task<DocumentGenerationRequestDto> GetDocumentAsync(
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken) =>
		_document ?? await loader.LoadAsync(RequestId, cancellationToken) ??
		throw new DocumentGenerationRequestNotFoundException(RequestId);
}
