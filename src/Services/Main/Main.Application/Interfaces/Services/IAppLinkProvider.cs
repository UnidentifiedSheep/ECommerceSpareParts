namespace Main.Application.Interfaces.Services;

public interface IAppLinkProvider
{
	Task<Uri> CreatePasswordResetUrlAsync(string token, CancellationToken cancellationToken = default);

	Task<Uri> CreateEmailVerificationUrlAsync(string token, CancellationToken cancellationToken = default);

	Task<Uri> CreateDocumentUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
}
