using Domain.CommonEnums;
using HotChocolate;
using Main.Application.Dtos.Documents;
using Main.Application.Interfaces.Cache;

namespace Main.Api.GraphQl.Types.Document;

[GraphQLName("DocumentGenerationRequest")]
public record GqlDocumentGenerationRequest(
	[property: GraphQLIgnore] DocumentGenerationRequestDto Dto)
{
	[GraphQLName("requestId")]
	public Guid RequestId => Dto.RequestId;

	[GraphQLName("jobId")]
	public Guid JobId => Dto.JobId;

	[GraphQLName("status")]
	public JobStatus Status => Dto.Status;

	[GraphQLName("documentSystemName")]
	public string DocumentSystemName => Dto.DocumentSystemName;

	[GraphQLName("requesterId")]
	public Guid? RequesterId => Dto.RequesterId;

	[GraphQLName("createdAtUtc")]
	public DateTime CreatedAtUtc => Dto.CreatedAtUtc;

	[GraphQLName("generatedAtUtc")]
	public DateTime? GeneratedAtUtc => Dto.GeneratedAtUtc;

	[GraphQLName("expiresAtUtc")]
	public DateTime? ExpiresAtUtc => Dto.ExpiresAtUtc;

	[GraphQLName("fileLink")]
	public async Task<GqlDocumentLink?> GetFileLinkAsync(
		IDocumentLinkProvider linkProvider)
	{
		if (Dto.BucketName == null || Dto.StorageKey == null || ExpiresAtUtc == null)
			return null;

		var res = await linkProvider.GetOrCreateAsync(
			RequestId,
			Dto.BucketName,
			Dto.StorageKey,
			ExpiresAtUtc.Value);

		return new GqlDocumentLink(res.Url, res.UrlExpiresAtUtc);
	}
}
