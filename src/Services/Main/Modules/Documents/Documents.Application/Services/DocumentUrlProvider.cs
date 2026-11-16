using Application.Common.Interfaces.Links;
using Main.Application.Interfaces.Services.Document;
using Main.Application.Models.Options;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

public sealed class DocumentUrlProvider(
	IAppLinkBuilder linkBuilder,
	IOptions<DocumentLinksOptions> options) : IDocumentUrlProvider
{
	public Task<Uri> CreateDocumentUrlAsync(Guid requestId, CancellationToken cancellationToken = default)
		=> linkBuilder.CreateAsync(
			options.Value.Document,
			new Dictionary<string, string>
			{
				["requestId"] = requestId.ToString("D")
			},
			cancellationToken);
}
