namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentUrlProvider
{
	Task<Uri> CreateDocumentUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
}
