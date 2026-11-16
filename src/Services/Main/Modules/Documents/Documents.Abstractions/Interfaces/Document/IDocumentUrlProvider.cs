namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentUrlProvider
{
	Task<Uri> CreateDocumentUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
}
