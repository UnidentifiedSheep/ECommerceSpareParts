namespace Main.Application.Interfaces.Services;

public interface IAppLinkProvider : IAuthLinkProvider
{
	Task<Uri> CreateDocumentUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
}
