namespace Main.Application.Interfaces.Services;

public interface IAuthLinkProvider
{
	Task<Uri> CreatePasswordResetUrlAsync(string token, CancellationToken cancellationToken = default);

	Task<Uri> CreateEmailVerificationUrlAsync(string token, CancellationToken cancellationToken = default);
}
